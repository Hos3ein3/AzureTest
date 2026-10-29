using System;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Trading.Events;

public sealed record TradingPositionClosedDomainEvent(
    Guid PositionId,
    Guid AccountId,
    Guid UserId,
    Guid TenantId,
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    TradingOrderSide Side,
    decimal Quantity,
    decimal EntryPrice,
    decimal ClosePrice,
    decimal InitialMargin,
    decimal RealizedPnl,
    decimal RoiPercentage,
    string TriggerReason,
    DateTime ClosedAtUtc
) : DomainEvent;
