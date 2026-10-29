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
using Microsoft.Extensions.Logging;

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
    private readonly IEventDispatcher? _eventDispatcher;
    private readonly Microsoft.Extensions.Logging.ILogger<SyncTradingPricesCommandHandler> _logger;

    public SyncTradingPricesCommandHandler(
        IUnitOfWork uow,
        ITradingMarketDataService marketData,
        IEventDispatcher? eventDispatcher = null,
        Microsoft.Extensions.Logging.ILogger<SyncTradingPricesCommandHandler>? logger = null)
    {
        _uow = uow;
        _marketData = marketData;
        _eventDispatcher = eventDispatcher;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<SyncTradingPricesCommandHandler>.Instance;
    }

    public async Task<Result<int>> HandleAsync(SyncTradingPricesCommand cmd, CancellationToken ct)
    {
        // 1. Check for open positions and pending orders first
        var positions = (await _uow.TradingPositions.GetAllAsync())
            .Where(p => p.Status == TradingPositionStatus.Open)
            .ToList();

        var pendingOrders = (await _uow.TradingOrders.GetAllAsync())
            .Where(o => o.Status == TradingOrderStatus.Pending)
            .ToList();

        // If there are no open positions and no pending orders, no external network fetch is needed!
        if (positions.Count == 0 && pendingOrders.Count == 0)
        {
            return Result<int>.Success(0);
        }

        // Only fetch market data for the EXACT active symbols that have open positions or pending orders!
        var activeSymbols = positions.Select(p => p.Symbol)
            .Concat(pendingOrders.Select(o => o.Symbol))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        await _marketData.RefreshSymbolsDataAsync(activeSymbols, ct);

        int actionsExecuted = 0;

        foreach (var pos in positions)
        {
            var currentPrice = await _marketData.GetCurrentPriceAsync(pos.Symbol, pos.CurrentPrice, ct);
            pos.UpdateCurrentPrice(currentPrice);

            bool shouldClose = false;
            bool isLiquidated = false;
            decimal closePrice = currentPrice;

            // Check Liquidation Trigger (floating loss consumed 100% of initial margin)
            if (pos.UnrealizedPnl <= -pos.InitialMargin)
            {
                shouldClose = true;
                isLiquidated = true;
                closePrice = currentPrice;
            }

            // Check Stop Loss Trigger
            if (!shouldClose && pos.StopLoss.HasValue && pos.StopLoss.Value > 0)
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
                var triggerReason = isLiquidated ? "Liquidation" : ((pos.TakeProfit.HasValue && closePrice == pos.TakeProfit.Value) ? "Take Profit" : "Stop Loss");

                if (isLiquidated)
                {
                    pos.Liquidate(closePrice);
                }
                else
                {
                    pos.Close(closePrice, triggerReason);
                }

                actionsExecuted++;

                _logger.LogInformation(
                    "Paper Trade Triggered via Domain Event: PositionId={PositionId}, Symbol={Symbol}, Side={Side}, Qty={Quantity}, EntryPrice={EntryPrice}, ExitPrice={ExitPrice}, RealizedPnl={RealizedPnl}, Reason={Reason}, ClosedAt={ClosedAtUtc}",
                    pos.Id, pos.Symbol, pos.Side, pos.Quantity, pos.EntryPrice, closePrice, pos.RealizedPnl, triggerReason, pos.ClosedAtUtc);
            }
        }

        // Check Pending Orders for Trigger / Fill
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
                        order.Fill(position.Id, marketPrice);
                        _uow.TradingPositions.Add(position);
                        actionsExecuted++;
                    }
                }
            }
        }

        if (actionsExecuted > 0 || positions.Count > 0)
        {
            // Atomically commit database changes and Outbox messages in single transaction
            await _uow.SaveChangesAsync(ct);

            // Dispatch collected domain events via EventDispatcher to invoke handlers
            var domainEvents = _uow.PopDomainEvents();
            if ((domainEvents == null || domainEvents.Count == 0))
            {
                domainEvents = positions.SelectMany(p => p.DomainEvents)
                    .Concat(pendingOrders.SelectMany(o => o.DomainEvents))
                    .ToList();
            }

            if (_eventDispatcher != null && domainEvents != null)
            {
                foreach (var domainEvent in domainEvents)
                {
                    await _eventDispatcher.PublishAsync(domainEvent, ct);
                }
            }
            else if (domainEvents != null)
            {
                var closedHandler = new AzureTest.Application.Trading.EventHandlers.TradingPositionClosedDomainEventHandler(_uow);
                var liquidatedHandler = new AzureTest.Application.Trading.EventHandlers.TradingPositionLiquidatedDomainEventHandler(_uow);
                var filledHandler = new AzureTest.Application.Trading.EventHandlers.TradingOrderFilledDomainEventHandler(_uow);

                foreach (var ev in domainEvents)
                {
                    if (ev is AzureTest.Domain.Entities.Trading.Events.TradingPositionClosedDomainEvent closed)
                        await closedHandler.HandleAsync(closed, ct);
                    else if (ev is AzureTest.Domain.Entities.Trading.Events.TradingPositionLiquidatedDomainEvent liq)
                        await liquidatedHandler.HandleAsync(liq, ct);
                    else if (ev is AzureTest.Domain.Entities.Trading.Events.TradingOrderFilledDomainEvent filled)
                        await filledHandler.HandleAsync(filled, ct);
                }
            }
        }

        return Result<int>.Success(actionsExecuted);
    }
}
