using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Crypto.Services;

public class CryptoExecutionService : ICryptoExecutionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICryptoMarketDataService _marketData;
    private readonly ICryptoRiskService _riskService;
    private readonly ILogger<CryptoExecutionService> _logger;

    public CryptoExecutionService(
        IUnitOfWork uow,
        ICryptoMarketDataService marketData,
        ICryptoRiskService riskService,
        ILogger<CryptoExecutionService> logger)
    {
        _uow = uow;
        _marketData = marketData;
        _riskService = riskService;
        _logger = logger;
    }

    public async Task<Result<CryptoOrder>> ExecutePaperOrderAsync(CryptoOrder order, CryptoAccount account, CancellationToken ct = default)
    {
        // 1. Idempotency Check (Safe Retry)
        if (!string.IsNullOrWhiteSpace(order.IdempotencyKey))
        {
            var existingOrders = await _uow.CryptoOrders.FindAsync(o => o.IdempotencyKey == order.IdempotencyKey);
            var existing = existingOrders.FirstOrDefault();
            if (existing != null)
            {
                _logger.LogInformation("Idempotent order matched: {Key}, returning existing order {Id}", order.IdempotencyKey, existing.Id);
                return Result<CryptoOrder>.Success(existing);
            }
        }

        // 2. Kill switch check
        if (account.IsKillSwitchActive)
        {
            order.Reject("Trading halted: Kill switch is active.");
            _uow.CryptoOrders.Add(order);
            await _uow.SaveChangesAsync(ct);
            return Result<CryptoOrder>.Failure("Trading is halted because the kill switch is active.");
        }

        // 3. Get live market price for realistic fill
        decimal currentMarketPrice = await _marketData.GetCurrentPriceAsync(order.Symbol, ct);
        if (currentMarketPrice <= 0m)
        {
            currentMarketPrice = order.RequestedPrice;
        }

        // 4. Check order type and fill conditions
        decimal fillPrice = currentMarketPrice;
        decimal slippage = 0m;
        decimal commission = 0m;

        if (order.OrderType == CryptoOrderType.Market)
        {
            // Apply slight realistic market slippage (0.03% - 0.08%)
            decimal slipPct = order.Side == CryptoOrderSide.Buy ? 0.0005m : -0.0005m;
            fillPrice = currentMarketPrice * (1.0m + slipPct);
            slippage = Math.Abs(fillPrice - currentMarketPrice);
            commission = order.Quantity * fillPrice * 0.00075m; // 0.075% taker fee
            order.Fill(fillPrice, order.Quantity, commission, slippage);
        }
        else if (order.OrderType == CryptoOrderType.Limit)
        {
            // Limit order logic: Buy fills if Market <= Limit; Sell fills if Market >= Limit
            bool canFill = order.Side == CryptoOrderSide.Buy
                ? currentMarketPrice <= order.RequestedPrice
                : currentMarketPrice >= order.RequestedPrice;

            if (canFill)
            {
                fillPrice = order.RequestedPrice;
                commission = order.Quantity * fillPrice * 0.0005m; // 0.05% maker fee
                order.Fill(fillPrice, order.Quantity, commission, 0m);
            }
        }
        else if (order.OrderType == CryptoOrderType.StopLoss)
        {
            bool triggerStop = order.StopPrice.HasValue && (order.Side == CryptoOrderSide.Sell
                ? currentMarketPrice <= order.StopPrice.Value
                : currentMarketPrice >= order.StopPrice.Value);

            if (triggerStop)
            {
                fillPrice = currentMarketPrice;
                commission = order.Quantity * fillPrice * 0.00075m;
                order.Fill(fillPrice, order.Quantity, commission, 0m);
            }
        }

        _uow.CryptoOrders.Add(order);
        await _uow.SaveChangesAsync(ct);
        return Result<CryptoOrder>.Success(order);
    }

    public async Task<Result<CryptoPosition>> OpenPositionAsync(
        CryptoOrder filledOrder,
        CryptoAccount account,
        int leverage,
        decimal? stopLoss,
        decimal? takeProfit,
        decimal? trailingStopDistance,
        CancellationToken ct = default)
    {
        if (filledOrder.Status != CryptoOrderStatus.Filled || !filledOrder.FillPrice.HasValue)
        {
            return Result<CryptoPosition>.Failure("Position can only be opened from a filled order.");
        }

        var side = filledOrder.Side == CryptoOrderSide.Buy ? CryptoPositionSide.Long : CryptoPositionSide.Short;
        decimal fillPrice = filledOrder.FillPrice.Value;
        decimal requiredMargin = (filledOrder.Quantity * fillPrice) / Math.Max(1, leverage);

        // Check risk limits
        bool canOpen = await _riskService.CanOpenNewPositionAsync(account, requiredMargin, ct);
        if (!canOpen)
        {
            return Result<CryptoPosition>.Failure("Cannot open position: risk limits (daily loss, max positions, or margin) reached.");
        }

        var position = CryptoPosition.Create(
            accountId: account.Id,
            userId: account.UserId,
            tenantId: account.TenantId,
            symbol: filledOrder.Symbol,
            side: side,
            quantity: filledOrder.Quantity,
            entryPrice: fillPrice,
            leverage: leverage,
            stopLoss: stopLoss,
            takeProfit: takeProfit,
            trailingStopDistance: trailingStopDistance);

        _uow.CryptoPositions.Add(position);

        // Update account margin and deduct commission
        account.UpdateBalance(
            cashDelta: -filledOrder.Commission,
            marginUsed: account.MarginUsed + requiredMargin,
            unrealizedPnl: 0m);

        await _uow.SaveChangesAsync(ct);
        return Result<CryptoPosition>.Success(position);
    }

    public async Task<Result<decimal>> ClosePositionAsync(Guid positionId, decimal? marketPriceOverride, CancellationToken ct = default)
    {
        var position = await _uow.CryptoPositions.GetByIdAsync(positionId);
        if (position == null)
        {
            return Result<decimal>.Failure("Position not found.");
        }

        if (position.Status != CryptoPositionStatus.Open)
        {
            return Result<decimal>.Failure("Position is already closed.");
        }

        decimal closePrice = marketPriceOverride ?? await _marketData.GetCurrentPriceAsync(position.Symbol, ct);
        if (closePrice <= 0m) closePrice = position.CurrentPrice;

        position.Close(closePrice);

        // Update account balances
        var account = await _uow.CryptoAccounts.GetByIdAsync(position.AccountId);
        if (account != null)
        {
            decimal fee = position.Quantity * closePrice * 0.00075m;
            decimal netRealized = position.RealizedPnl - fee;
            decimal freedMargin = Math.Max(0m, account.MarginUsed - position.InitialMargin);

            account.UpdateBalance(
                cashDelta: netRealized,
                marginUsed: freedMargin,
                unrealizedPnl: 0m);
        }

        await _uow.SaveChangesAsync(ct);
        return Result<decimal>.Success(position.RealizedPnl);
    }

    public async Task<Result> UpdateStopsAsync(Guid positionId, decimal? stopLoss, decimal? takeProfit, decimal? trailingStopDistance, CancellationToken ct = default)
    {
        var position = await _uow.CryptoPositions.GetByIdAsync(positionId);
        if (position == null)
        {
            return Result.Failure("Position not found.");
        }

        position.UpdateStops(stopLoss, takeProfit, trailingStopDistance);
        await _uow.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<int>> TriggerKillSwitchAsync(Guid accountId, string reason, CancellationToken ct = default)
    {
        var account = await _uow.CryptoAccounts.GetByIdAsync(accountId);
        if (account == null)
        {
            return Result<int>.Failure("Account not found.");
        }

        // 1. Activate kill switch on account
        account.TriggerKillSwitch(reason);

        // 2. Cancel all pending orders
        var pendingOrders = (await _uow.CryptoOrders.FindAsync(o => o.AccountId == accountId && o.Status == CryptoOrderStatus.Pending)).ToList();
        foreach (var order in pendingOrders)
        {
            order.Cancel();
        }

        // 3. Market-close all open positions
        var openPositions = (await _uow.CryptoPositions.FindAsync(p => p.AccountId == accountId && p.Status == CryptoPositionStatus.Open)).ToList();
        int closedCount = 0;

        foreach (var position in openPositions)
        {
            decimal currentPrice = await _marketData.GetCurrentPriceAsync(position.Symbol, ct);
            if (currentPrice <= 0m) currentPrice = position.CurrentPrice;

            position.Close(currentPrice);
            closedCount++;
        }

        // Recalculate account equity with all margins released
        account.UpdateBalance(cashDelta: 0m, marginUsed: 0m, unrealizedPnl: 0m);

        await _uow.SaveChangesAsync(ct);
        return Result<int>.Success(closedCount);
    }
}
