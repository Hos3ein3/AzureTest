using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Domain.Entities.Trading.Enums;
using FluentValidation;

namespace AzureTest.Application.Trading.Commands.SyncTradingPrices;

public sealed record SyncTradingPricesCommand : ICommand<Result<int>>;

public sealed class SyncTradingPricesCommandValidator : AbstractValidator<SyncTradingPricesCommand>
{
    public SyncTradingPricesCommandValidator()
    {
    }
}

public sealed class SyncTradingPricesCommandHandler : ICommandHandler<SyncTradingPricesCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;
    private readonly AzureTest.Application.Interfaces.Services.IAppNotificationService? _notificationService;

    public SyncTradingPricesCommandHandler(
        IUnitOfWork uow,
        ITradingMarketDataService marketData,
        AzureTest.Application.Interfaces.Services.IAppNotificationService? notificationService = null)
    {
        _uow = uow;
        _marketData = marketData;
        _notificationService = notificationService;
    }

    public async Task<Result<int>> HandleAsync(SyncTradingPricesCommand cmd, CancellationToken ct)
    {
        // 1. Refresh live real-world data from Binance (Crypto) and Frankfurter ECB (Forex)
        await _marketData.RefreshRealWorldDataAsync(ct);

        var positions = (await _uow.TradingPositions.GetAllAsync())
            .Where(p => p.Status == TradingPositionStatus.Open)
            .ToList();

        int actionsExecuted = 0;

        foreach (var pos in positions)
        {
            var currentPrice = await _marketData.GetCurrentPriceAsync(pos.Symbol, pos.CurrentPrice, ct);
            pos.UpdateCurrentPrice(currentPrice);

            bool shouldClose = false;
            decimal closePrice = currentPrice;

            // Check Stop Loss Trigger
            if (pos.StopLoss.HasValue && pos.StopLoss.Value > 0)
            {
                if (pos.Side == TradingOrderSide.Buy && currentPrice <= pos.StopLoss.Value)
                {
                    shouldClose = true;
                    closePrice = pos.StopLoss.Value;
                }
                else if (pos.Side == TradingOrderSide.Sell && currentPrice >= pos.StopLoss.Value)
                {
                    shouldClose = true;
                    closePrice = pos.StopLoss.Value;
                }
            }

            // Check Take Profit Trigger
            if (!shouldClose && pos.TakeProfit.HasValue && pos.TakeProfit.Value > 0)
            {
                if (pos.Side == TradingOrderSide.Buy && currentPrice >= pos.TakeProfit.Value)
                {
                    shouldClose = true;
                    closePrice = pos.TakeProfit.Value;
                }
                else if (pos.Side == TradingOrderSide.Sell && currentPrice <= pos.TakeProfit.Value)
                {
                    shouldClose = true;
                    closePrice = pos.TakeProfit.Value;
                }
            }

            if (shouldClose)
            {
                pos.Close(closePrice);
                var account = await _uow.TradingAccounts.GetByIdAsync(pos.AccountId);
                if (account != null)
                {
                    account.ApplyRealizedPnl(pos.RealizedPnl, pos.InitialMargin);
                }
                actionsExecuted++;

                if (_notificationService != null)
                {
                    var triggerReason = (pos.TakeProfit.HasValue && closePrice == pos.TakeProfit.Value) ? "Take Profit" : "Stop Loss";
                    var pnlSign = pos.RealizedPnl >= 0 ? "+" : "";
                    _ = _notificationService.SendNotificationAsync(
                        pos.UserId,
                        pos.TenantId,
                        $"Paper Trade Closed ({triggerReason}): {pos.Symbol}",
                        $"Triggered {triggerReason} at ${closePrice:N2}. Realized PnL: {pnlSign}${pos.RealizedPnl:N2}.",
                        type: "Trade",
                        actionUrl: "/Trading",
                        ct: ct);
                }
            }
        }

        // Check Pending Orders for Trigger / Fill
        var pendingOrders = (await _uow.TradingOrders.GetAllAsync())
            .Where(o => o.Status == TradingOrderStatus.Pending)
            .ToList();

        foreach (var order in pendingOrders)
        {
            var marketPrice = await _marketData.GetCurrentPriceAsync(order.Symbol, 100m, ct);
            bool shouldFill = false;

            if (order.OrderType == TradingOrderType.Limit && order.Price.HasValue)
            {
                if (order.Side == TradingOrderSide.Buy && marketPrice <= order.Price.Value)
                    shouldFill = true;
                else if (order.Side == TradingOrderSide.Sell && marketPrice >= order.Price.Value)
                    shouldFill = true;
            }
            else if (order.OrderType == TradingOrderType.Stop && order.TriggerPrice.HasValue)
            {
                if (order.Side == TradingOrderSide.Buy && marketPrice >= order.TriggerPrice.Value)
                    shouldFill = true;
                else if (order.Side == TradingOrderSide.Sell && marketPrice <= order.TriggerPrice.Value)
                    shouldFill = true;
            }

            if (shouldFill)
            {
                var account = await _uow.TradingAccounts.GetByIdAsync(order.AccountId);
                if (account != null)
                {
                    var position = AzureTest.Domain.Entities.Trading.TradingPosition.Open(
                        order.AccountId,
                        order.UserId,
                        order.TenantId,
                        order.Symbol,
                        order.AssetName,
                        order.AssetClass,
                        order.Side,
                        order.Quantity,
                        marketPrice,
                        order.Leverage,
                        order.StopLoss,
                        order.TakeProfit,
                        order.MultipleTakeProfitsJson,
                        order.TrailingStopDistance);

                    if (account.FreeMargin >= position.InitialMargin)
                    {
                        account.DeductMargin(position.InitialMargin);
                        order.Fill();
                        _uow.TradingPositions.Add(position);
                        actionsExecuted++;

                        if (_notificationService != null)
                        {
                            var sideText = order.Side == TradingOrderSide.Buy ? "Buy (Long)" : "Sell (Short)";
                            _ = _notificationService.SendNotificationAsync(
                                order.UserId,
                                order.TenantId,
                                $"Paper Order Filled: {order.Symbol}",
                                $"Pending order filled: {sideText} {order.Quantity:G29} {order.Symbol} @ ${marketPrice:N2}.",
                                type: "Trade",
                                actionUrl: "/Trading",
                                ct: ct);
                        }
                    }
                }
            }
        }

        if (actionsExecuted > 0 || positions.Count > 0)
        {
            await _uow.SaveChangesAsync(ct);
        }

        return Result<int>.Success(actionsExecuted);
    }
}
