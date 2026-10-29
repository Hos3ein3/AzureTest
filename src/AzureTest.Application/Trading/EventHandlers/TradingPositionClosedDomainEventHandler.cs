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

public sealed class TradingPositionClosedDomainEventHandler : IEventHandler<TradingPositionClosedDomainEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IAppNotificationService? _notificationService;
    private readonly ILogger<TradingPositionClosedDomainEventHandler> _logger;

    public TradingPositionClosedDomainEventHandler(
        IUnitOfWork uow,
        IAppNotificationService? notificationService = null,
        ILogger<TradingPositionClosedDomainEventHandler>? logger = null)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TradingPositionClosedDomainEventHandler>.Instance;
    }

    public async Task HandleAsync(TradingPositionClosedDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var handlerName = nameof(TradingPositionClosedDomainEventHandler);

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

        // 2. Settle Account Balance (Aggregate Invariant)
        var account = await _uow.TradingAccounts.GetByIdAsync(@event.AccountId);
        if (account != null)
        {
            account.ApplyRealizedPnl(@event.RealizedPnl, @event.InitialMargin);
            _logger.LogInformation(
                "DomainEventHandler: Settled Realized PnL {RealizedPnl} on TradingAccount {AccountId} for closed position {PositionId}.",
                @event.RealizedPnl, @event.AccountId, @event.PositionId);
        }

        // 3. Dispatch Notification Side Effect
        if (_notificationService != null)
        {
            var formattedClosePrice = TradingNotificationFormatter.FormatPrice(@event.ClosePrice);
            var formattedPnl = TradingNotificationFormatter.FormatPnl(@event.RealizedPnl);
            var title = $"Paper Trade Closed ({@event.TriggerReason}): {@event.Symbol}";
            var message = $"Closed {@event.Side} position for {@event.Quantity:0.######} {@event.Symbol} @ {formattedClosePrice}. Realized PnL: {formattedPnl}.";

            var metadata = new Dictionary<string, string>
            {
                ["positionId"] = @event.PositionId.ToString(),
                ["symbol"] = @event.Symbol,
                ["side"] = @event.Side.ToString(),
                ["realizedPnl"] = @event.RealizedPnl.ToString(CultureInfo.InvariantCulture),
                ["closePrice"] = @event.ClosePrice.ToString(CultureInfo.InvariantCulture),
                ["exitPrice"] = @event.ClosePrice.ToString(CultureInfo.InvariantCulture),
                ["triggerReason"] = @event.TriggerReason
            };

            await _notificationService.SendNotificationAsync(
                @event.UserId,
                @event.TenantId,
                title,
                message,
                type: "Trade",
                actionUrl: "/Trading",
                metadata: metadata,
                ct: cancellationToken);
        }

        // 4. Record into Inbox to guarantee idempotency
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
