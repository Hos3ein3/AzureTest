using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Domain.Entities.Outbox;
using AzureTest.Domain.Entities.Trading.Events;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Trading.EventHandlers;

public sealed class TradingPositionLiquidatedDomainEventHandler : IEventHandler<TradingPositionLiquidatedDomainEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IAppNotificationService? _notificationService;
    private readonly ILogger<TradingPositionLiquidatedDomainEventHandler> _logger;

    public TradingPositionLiquidatedDomainEventHandler(
        IUnitOfWork uow,
        IAppNotificationService? notificationService = null,
        ILogger<TradingPositionLiquidatedDomainEventHandler>? logger = null)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TradingPositionLiquidatedDomainEventHandler>.Instance;
    }

    public async Task HandleAsync(TradingPositionLiquidatedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var handlerName = nameof(TradingPositionLiquidatedDomainEventHandler);

        // 1. Idempotency Check via Inbox Pattern
        if (_uow.InboxMessages != null)
        {
            var alreadyProcessed = await _uow.InboxMessages.HasBeenProcessedAsync(@event.Id, handlerName, cancellationToken);
            if (alreadyProcessed)
            {
                _logger.LogInformation("Domain event {EventId} was already processed by {HandlerName}. Skipping duplicate.", @event.Id, handlerName);
                return;
            }
        }

        // 2. Settle Liquidation Loss on TradingAccount
        var account = await _uow.TradingAccounts.GetByIdAsync(@event.AccountId);
        if (account != null)
        {
            account.ApplyRealizedPnl(-@event.InitialMarginLoss, @event.InitialMarginLoss);
            _logger.LogWarning(
                "DomainEventHandler: TradingPosition {PositionId} was LIQUIDATED at {LiquidationPrice}. Margin loss {Loss} applied to Account {AccountId}.",
                @event.PositionId, @event.LiquidationPrice, @event.InitialMarginLoss, @event.AccountId);
        }

        // 3. Dispatch High-Priority Liquidation Alert
        if (_notificationService != null)
        {
            var formattedPrice = TradingNotificationFormatter.FormatPrice(@event.LiquidationPrice);
            var formattedLoss = TradingNotificationFormatter.FormatPnl(-@event.InitialMarginLoss);
            var title = $"Paper Trade LIQUIDATED: {@event.Symbol}";
            var message = $"Position liquidated at {formattedPrice}. Initial margin consumed: {formattedLoss}.";

            var metadata = new Dictionary<string, string>
            {
                ["positionId"] = @event.PositionId.ToString(),
                ["symbol"] = @event.Symbol,
                ["liquidationPrice"] = @event.LiquidationPrice.ToString(CultureInfo.InvariantCulture),
                ["lossAmount"] = @event.InitialMarginLoss.ToString(CultureInfo.InvariantCulture),
                ["isLiquidated"] = "true"
            };

            await _notificationService.SendNotificationAsync(
                @event.UserId,
                @event.TenantId,
                title,
                message,
                type: "TradeAlert",
                actionUrl: "/Trading",
                metadata: metadata,
                ct: cancellationToken);
        }

        // 4. Record into Inbox
        if (_uow.InboxMessages != null)
        {
            await _uow.InboxMessages.AddAsync(new InboxMessage
            {
                Id = @event.Id,
                HandlerName = handlerName,
                ReceivedOnUtc = DateTime.UtcNow,
                ProcessedOnUtc = DateTime.UtcNow
            }, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
