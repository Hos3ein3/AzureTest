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

    public SyncTradingPricesCommandHandler(IUnitOfWork uow, ITradingMarketDataService marketData)
    {
        _uow = uow;
        _marketData = marketData;
    }

    public async Task<Result<int>> HandleAsync(SyncTradingPricesCommand cmd, CancellationToken ct)
    {
        var positions = (await _uow.TradingPositions.GetAllAsync())
            .Where(p => p.Status == TradingPositionStatus.Open)
            .ToList();

        int actionsExecuted = 0;

        foreach (var pos in positions)
        {
            var currentPrice = _marketData.GetCurrentPrice(pos.Symbol, pos.CurrentPrice);
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
            }
        }

        // Check Pending Orders for Trigger / Fill
        var pendingOrders = (await _uow.TradingOrders.GetAllAsync())
            .Where(o => o.Status == TradingOrderStatus.Pending)
            .ToList();

        foreach (var order in pendingOrders)
        {
            var marketPrice = _marketData.GetCurrentPrice(order.Symbol, 100m);
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
