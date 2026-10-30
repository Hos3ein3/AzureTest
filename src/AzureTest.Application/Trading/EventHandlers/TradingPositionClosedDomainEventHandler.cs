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

public sealed class TradingPositionClosedDomainEventHandler
    : IdempotentEventHandler<TradingPositionClosedDomainEvent>
{
    private readonly IAppNotificationService _notifications;

    public TradingPositionClosedDomainEventHandler(
        IUnitOfWork uow,
        IAppNotificationService notifications,
        ILogger<TradingPositionClosedDomainEventHandler> logger,
        TimeProvider time) : base(uow, logger, time)
        => _notifications = notifications;

    protected override async Task ApplyAsync(TradingPositionClosedDomainEvent e, CancellationToken ct)
    {
        var account = await Uow.TradingAccounts.GetByIdAsync(e.AccountId, ct);
        if (account is null)
        {
            Logger.LogWarning("TradingAccount {AccountId} not found for closed position {PositionId}.", e.AccountId, e.PositionId);
            return;
        }

        account.ApplyRealizedPnl(e.RealizedPnl, e.InitialMargin);
        Logger.LogInformation("Settled PnL {Pnl} on account {AccountId} for position {PositionId}.",
            e.RealizedPnl, e.AccountId, e.PositionId);
    }

    protected override Task NotifyAsync(TradingPositionClosedDomainEvent e, CancellationToken ct)
    {
        var message = $"Closed {e.Side} position for {e.Quantity:0.######} {e.Symbol} @ " +
                      $"{TradingNotificationFormatter.FormatPrice(e.ClosePrice)}. " +
                      $"Realized PnL: {TradingNotificationFormatter.FormatPnl(e.RealizedPnl)}.";

        var close = e.ClosePrice.ToString(CultureInfo.InvariantCulture);
        var metadata = new Dictionary<string, string>
        {
            ["positionId"] = e.PositionId.ToString(),
            ["symbol"] = e.Symbol,
            ["side"] = e.Side.ToString(),
            ["realizedPnl"] = e.RealizedPnl.ToString(CultureInfo.InvariantCulture),
            ["closePrice"] = close,
            ["exitPrice"] = close,
            ["triggerReason"] = e.TriggerReason
        };

        return _notifications.SendNotificationAsync(e.UserId, e.TenantId,
            $"Paper Trade Closed ({e.TriggerReason}): {e.Symbol}", message,
            type: "Trade", actionUrl: "/Trading", metadata: metadata, ct: ct);
    }
}
