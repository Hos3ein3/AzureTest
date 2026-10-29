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
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Domain.Entities.Trading.Events;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Trading.EventHandlers;

public sealed class TradingOrderFilledDomainEventHandler : IEventHandler<TradingOrderFilledDomainEvent>
{
    private readonly IUnitOfWork _uow;
    private readonly IAppNotificationService? _notificationService;
    private readonly ILogger<TradingOrderFilledDomainEventHandler> _logger;

    public TradingOrderFilledDomainEventHandler(
        IUnitOfWork uow,
        IAppNotificationService? notificationService = null,
        ILogger<TradingOrderFilledDomainEventHandler>? logger = null)
    {
        _uow = uow;
        _notificationService = notificationService;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TradingOrderFilledDomainEventHandler>.Instance;
    }

    public async Task HandleAsync(TradingOrderFilledDomainEvent @event, CancellationToken cancellationToken = default)
    {
        var handlerName = nameof(TradingOrderFilledDomainEventHandler);

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

        // 2. Dispatch Order Filled Notification
        if (_notificationService != null)
        {
            var sideText = @event.Side == TradingOrderSide.Buy ? "Buy (Long)" : "Sell (Short)";
            var formattedPrice = TradingNotificationFormatter.FormatPrice(@event.FillPrice);
            var formattedQty = TradingNotificationFormatter.FormatQuantity(@event.Quantity);
            var title = $"Paper Order Filled: {@event.Symbol}";
            var message = $"Pending order filled: {sideText} {formattedQty} {@event.Symbol} @ {formattedPrice} (Leverage: {@event.Leverage}x).";

            var metadata = new Dictionary<string, string>
            {
                ["orderId"] = @event.OrderId.ToString(),
                ["positionId"] = @event.PositionId.ToString(),
                ["symbol"] = @event.Symbol,
                ["fillPrice"] = @event.FillPrice.ToString(CultureInfo.InvariantCulture),
                ["quantity"] = @event.Quantity.ToString(CultureInfo.InvariantCulture)
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

        // 3. Record into Inbox
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
