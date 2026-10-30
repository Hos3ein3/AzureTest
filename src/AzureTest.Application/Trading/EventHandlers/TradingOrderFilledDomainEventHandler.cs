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
public sealed class TradingOrderFilledDomainEventHandler
    : IdempotentEventHandler<TradingOrderFilledDomainEvent>
{
    private readonly IAppNotificationService _notifications;

    public TradingOrderFilledDomainEventHandler(
        IUnitOfWork uow,
        IAppNotificationService notifications,
        ILogger<TradingOrderFilledDomainEventHandler> logger,
        TimeProvider time) : base(uow, logger, time)
        => _notifications = notifications;

    protected override Task NotifyAsync(TradingOrderFilledDomainEvent e, CancellationToken ct)
    {
        var sideText = e.Side == TradingOrderSide.Buy ? "Buy (Long)" : "Sell (Short)";
        var message = $"Pending order filled: {sideText} {TradingNotificationFormatter.FormatQuantity(e.Quantity)} " +
                      $"{e.Symbol} @ {TradingNotificationFormatter.FormatPrice(e.FillPrice)} (Leverage: {e.Leverage}x).";

        var metadata = new Dictionary<string, string>
        {
            ["orderId"] = e.OrderId.ToString(),
            ["positionId"] = e.PositionId.ToString(),
            ["symbol"] = e.Symbol,
            ["fillPrice"] = e.FillPrice.ToString(CultureInfo.InvariantCulture),
            ["quantity"] = e.Quantity.ToString(CultureInfo.InvariantCulture)
        };

        return _notifications.SendNotificationAsync(e.UserId, e.TenantId,
            $"Paper Order Filled: {e.Symbol}", message,
            type: "Trade", actionUrl: "/Trading", metadata: metadata, ct: ct);
    }
}
