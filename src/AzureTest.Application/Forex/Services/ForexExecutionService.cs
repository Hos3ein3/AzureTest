using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.Forex;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Forex.Services;

public sealed class ForexExecutionService : IForexExecutionService
{
    private readonly IForexMarketDataService _marketData;
    private readonly IForexOrderRepository _orderRepo;
    private readonly IForexTradeRepository _tradeRepo;
    private readonly ILogger<ForexExecutionService> _logger;

    public ForexExecutionService(
        IForexMarketDataService marketData,
        IForexOrderRepository orderRepo,
        IForexTradeRepository tradeRepo,
        ILogger<ForexExecutionService> logger)
    {
        _marketData = marketData;
        _orderRepo = orderRepo;
        _tradeRepo = tradeRepo;
        _logger = logger;
    }

    public async Task<Result<ForexTrade>> ExecuteOrderAsync(
        ForexAccount account,
        string symbol,
        ForexTradeDirection direction,
        ForexOrderType orderType,
        decimal lotSize,
        decimal requestedPrice,
        decimal? stopLoss,
        decimal? takeProfit,
        string idempotencyKey,
        CancellationToken cancellationToken = default)
    {
        // 1. Safe retry idempotency check
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingOrders = await _orderRepo.FindAsync(o => o.IdempotencyKey == idempotencyKey);
            var existingOrder = existingOrders.FirstOrDefault();
            if (existingOrder != null)
            {
                _logger.LogWarning("Safe retry intercepted duplicate order submission with IdempotencyKey {Key}", idempotencyKey);
                var existingTrades = await _tradeRepo.FindAsync(t => t.OrderId == existingOrder.Id);
                var existingTrade = existingTrades.FirstOrDefault();
                if (existingTrade != null) return Result<ForexTrade>.Success(existingTrade);
            }
        }

        // 2. Fetch live price
        var quote = await _marketData.GetQuoteAsync(symbol, cancellationToken);
        var isJpy = symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);
        var pipMultiplier = isJpy ? 0.01m : (isGold ? 0.1m : 0.0001m);

        decimal executionPrice = direction == ForexTradeDirection.Buy ? quote.Ask : quote.Bid;
        if (orderType == ForexOrderType.Market)
        {
            // Slight realistic execution slippage (0.1 to 0.4 pips)
            var rng = new Random();
            var slippagePips = Math.Round((decimal)rng.NextDouble() * 0.3m, 1);
            var slippageOffset = slippagePips * pipMultiplier;

            executionPrice = direction == ForexTradeDirection.Buy
                ? executionPrice + slippageOffset
                : executionPrice - slippageOffset;
        }

        // 3. Margin requirement calculation
        // Standard lot = 100,000 units. Notional = lotSize * 100,000 * executionPrice
        var notional = isGold ? lotSize * 100m * executionPrice : lotSize * 100_000m;
        var leverage = account.Leverage > 0 ? account.Leverage : 30;
        var requiredMargin = Math.Round(notional / leverage, 2);

        if (account.FreeMargin < requiredMargin)
        {
            return Result<ForexTrade>.Failure("Insufficient free margin to execute order. Consider reducing lot size.");
        }

        // 4. Create Order and Mark Filled
        var order = ForexOrder.Create(
            account.Id,
            account.UserId,
            symbol,
            direction,
            orderType,
            lotSize,
            requestedPrice > 0 ? requestedPrice : executionPrice,
            stopLoss,
            takeProfit,
            idempotencyKey,
            account.TenantId
        );

        order.MarkFilled(executionPrice, 0.2m);
        _orderRepo.Add(order);

        // 5. Create Trade
        var trade = ForexTrade.Create(
            account.Id,
            account.UserId,
            order.Id,
            symbol,
            direction,
            lotSize,
            executionPrice,
            stopLoss,
            takeProfit,
            0m,
            requiredMargin,
            account.TenantId
        );

        account.ReserveMargin(requiredMargin);
        _tradeRepo.Add(trade);

        return Result<ForexTrade>.Success(trade);
    }

    public async Task<Result<decimal>> ClosePositionAsync(
        ForexAccount account,
        ForexTrade trade,
        decimal currentMarketPrice,
        ForexExitReason reason,
        CancellationToken cancellationToken = default)
    {
        if (trade.Status != ForexTradeStatus.Open)
        {
            return Result<decimal>.Failure("Trade is already closed.");
        }

        var isJpy = trade.Symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = trade.Symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);
        var pipMultiplier = isJpy ? 0.01m : (isGold ? 0.1m : 0.0001m);
        var pipValueStandardLot = isJpy ? 6.55m : 10.00m;

        // Calculate pips
        decimal pips = trade.Direction == ForexTradeDirection.Buy
            ? (currentMarketPrice - trade.OpenPrice) / pipMultiplier
            : (trade.OpenPrice - currentMarketPrice) / pipMultiplier;

        pips = Math.Round(pips, 1);

        decimal grossProfit = Math.Round(pips * pipValueStandardLot * trade.LotSize, 2);
        decimal commission = Math.Round(trade.LotSize * 5.0m, 2); // $5 round-turn per lot
        decimal swap = Math.Round(trade.LotSize * 1.5m, 2);

        trade.Close(currentMarketPrice, pips, grossProfit, swap, commission, reason);
        account.ApplyClosedTrade(trade.NetProfit, trade.MarginRequired);

        return Result<decimal>.Success(trade.NetProfit);
    }

    public async Task<Result<decimal>> ExecutePartialCloseAsync(
        ForexAccount account,
        ForexTrade trade,
        decimal lotsToClose,
        decimal currentMarketPrice,
        CancellationToken cancellationToken = default)
    {
        if (trade.Status != ForexTradeStatus.Open)
        {
            return Result<decimal>.Failure("Cannot partially close: Trade is already closed.");
        }

        if (lotsToClose <= 0)
        {
            return Result<decimal>.Failure("Invalid lot size to close.");
        }

        if (lotsToClose >= trade.LotSize)
        {
            // Close entire position if closing amount is equal to or exceeds current lot size
            return await ClosePositionAsync(account, trade, currentMarketPrice, ForexExitReason.TakeProfit, cancellationToken);
        }

        var isJpy = trade.Symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
        var isGold = trade.Symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);
        var pipMultiplier = isJpy ? 0.01m : (isGold ? 0.1m : 0.0001m);
        var pipValueStandardLot = isJpy ? 6.55m : 10.00m;

        decimal pips = trade.Direction == ForexTradeDirection.Buy
            ? (currentMarketPrice - trade.OpenPrice) / pipMultiplier
            : (trade.OpenPrice - currentMarketPrice) / pipMultiplier;
        pips = Math.Round(pips, 1);

        decimal grossProfit = Math.Round(pips * pipValueStandardLot * lotsToClose, 2);
        decimal commission = Math.Round(lotsToClose * 5.0m, 2);
        decimal swap = Math.Round(lotsToClose * 1.5m, 2);
        decimal partialNetProfit = grossProfit - swap - commission;

        decimal freedMargin = Math.Round(trade.MarginRequired * (lotsToClose / trade.LotSize), 2);

        trade.ReduceLotSize(lotsToClose);
        account.ApplyClosedTrade(partialNetProfit, freedMargin);
        _tradeRepo.Update(trade);

        _logger.LogInformation("Executed partial close on trade {TradeId}: closed {Lots} lots, realized profit {Profit}", trade.Id, lotsToClose, partialNetProfit);

        return Result<decimal>.Success(partialNetProfit);
    }

    public Task<Result<bool>> MoveToBreakEvenAsync(
        ForexAccount account,
        ForexTrade trade,
        decimal currentMarketPrice,
        CancellationToken cancellationToken = default)
    {
        if (trade.Status != ForexTradeStatus.Open)
        {
            return Task.FromResult(Result<bool>.Failure("Trade is not open."));
        }

        if (trade.BreakEvenTriggered)
        {
            return Task.FromResult(Result<bool>.Success(true));
        }

        trade.TriggerBreakEven(1.0m);
        _tradeRepo.Update(trade);

        _logger.LogInformation("Moved trade {TradeId} to Break-Even at price {StopLoss}", trade.Id, trade.StopLoss);

        return Task.FromResult(Result<bool>.Success(true));
    }

    public async Task<Result<int>> ExecuteKillSwitchAsync(
        ForexAccount account,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var openTrades = (await _tradeRepo.FindAsync(t => t.AccountId == account.Id && t.Status == ForexTradeStatus.Open)).ToList();
        int closedCount = 0;

        foreach (var trade in openTrades)
        {
            var quote = await _marketData.GetQuoteAsync(trade.Symbol, cancellationToken);
            decimal exitPrice = trade.Direction == ForexTradeDirection.Buy ? quote.Bid : quote.Ask;
            var res = await ClosePositionAsync(account, trade, exitPrice, ForexExitReason.EmergencyStop, cancellationToken);
            if (res.IsSuccess)
            {
                closedCount++;
            }
        }

        _logger.LogWarning("EMERGENCY KILL SWITCH ACTIVATED for Account {AccountId}: {Reason}. Closed {Count} positions.", account.Id, reason, closedCount);

        return Result<int>.Success(closedCount);
    }

    public async Task<Result<Guid>> ProcessTradingViewWebhookAsync(
        TradingViewWebhookPayloadDto payload,
        ForexAccount account,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payload.SecretToken) || payload.SecretToken != "AZURETEST_SECURE_WEBHOOK_KEY_2026")
        {
            return Result<Guid>.Failure("Invalid webhook secret token authentication failed.");
        }

        var action = payload.Action.ToUpperInvariant().Trim();
        var symbol = payload.Ticker.Replace("/", "").Trim().ToUpperInvariant();

        if (action is "BUY" or "LONG")
        {
            var res = await ExecuteOrderAsync(
                account,
                symbol,
                ForexTradeDirection.Buy,
                ForexOrderType.Market,
                payload.LotSize > 0 ? payload.LotSize : 0.10m,
                payload.Price ?? 0m,
                payload.StopLoss,
                payload.TakeProfit,
                Guid.NewGuid().ToString("N"),
                cancellationToken
            );
            return res.IsSuccess ? Result<Guid>.Success(res.Value.Id) : Result<Guid>.Failure(res.Error);
        }
        else if (action is "SELL" or "SHORT")
        {
            var res = await ExecuteOrderAsync(
                account,
                symbol,
                ForexTradeDirection.Sell,
                ForexOrderType.Market,
                payload.LotSize > 0 ? payload.LotSize : 0.10m,
                payload.Price ?? 0m,
                payload.StopLoss,
                payload.TakeProfit,
                Guid.NewGuid().ToString("N"),
                cancellationToken
            );
            return res.IsSuccess ? Result<Guid>.Success(res.Value.Id) : Result<Guid>.Failure(res.Error);
        }
        else if (action is "CLOSE" or "FLATTEN")
        {
            var openTrades = (await _tradeRepo.FindAsync(t => t.AccountId == account.Id && t.Status == ForexTradeStatus.Open && t.Symbol == symbol)).ToList();
            foreach (var t in openTrades)
            {
                var quote = await _marketData.GetQuoteAsync(symbol, cancellationToken);
                decimal exitPrice = t.Direction == ForexTradeDirection.Buy ? quote.Bid : quote.Ask;
                await ClosePositionAsync(account, t, exitPrice, ForexExitReason.SignalExit, cancellationToken);
            }
            return Result<Guid>.Success(Guid.Empty);
        }

        return Result<Guid>.Failure($"Unsupported webhook action '{payload.Action}'.");
    }
}
