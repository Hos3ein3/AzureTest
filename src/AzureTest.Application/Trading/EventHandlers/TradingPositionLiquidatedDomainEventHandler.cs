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

public sealed class TradingPositionLiquidatedDomainEventHandler
    : IdempotentEventHandler<TradingPositionLiquidatedDomainEvent>
{
    private readonly IAppNotificationService _notifications;

    public TradingPositionLiquidatedDomainEventHandler(
        IUnitOfWork uow,
        IAppNotificationService notifications,
        ILogger<TradingPositionLiquidatedDomainEventHandler> logger,
        TimeProvider time) : base(uow, logger, time)
        => _notifications = notifications;

    protected override async Task ApplyAsync(TradingPositionLiquidatedDomainEvent e, CancellationToken ct)
    {
        var account = await Uow.TradingAccounts.GetByIdAsync(e.AccountId, ct);
        if (account is null)
        {
            Logger.LogWarning("TradingAccount {AccountId} not found for liquidated position {PositionId}.", e.AccountId, e.PositionId);
            return;
        }

        account.ApplyRealizedPnl(-e.InitialMarginLoss, e.InitialMarginLoss);
        Logger.LogWarning("Position {PositionId} LIQUIDATED at {Price}. Loss {Loss} applied to account {AccountId}.",
            e.PositionId, e.LiquidationPrice, e.InitialMarginLoss, e.AccountId);
    }

    protected override Task NotifyAsync(TradingPositionLiquidatedDomainEvent e, CancellationToken ct)
    {
        var message = $"Position liquidated at {TradingNotificationFormatter.FormatPrice(e.LiquidationPrice)}. " +
                      $"Initial margin consumed: {TradingNotificationFormatter.FormatPnl(-e.InitialMarginLoss)}.";

        var metadata = new Dictionary<string, string>
        {
            ["positionId"] = e.PositionId.ToString(),
            ["symbol"] = e.Symbol,
            ["liquidationPrice"] = e.LiquidationPrice.ToString(CultureInfo.InvariantCulture),
            ["lossAmount"] = e.InitialMarginLoss.ToString(CultureInfo.InvariantCulture),
            ["isLiquidated"] = "true"
        };

        return _notifications.SendNotificationAsync(e.UserId, e.TenantId,
            $"Paper Trade LIQUIDATED: {e.Symbol}", message,
            type: "TradeAlert", actionUrl: "/Trading", metadata: metadata, ct: ct);
    }
}