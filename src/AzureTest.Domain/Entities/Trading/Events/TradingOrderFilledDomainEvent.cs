using System;
using AzureTest.Domain.Entities.Trading.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Trading.Events;

public sealed record TradingOrderFilledDomainEvent(
    Guid OrderId,
    Guid PositionId,
    Guid AccountId,
    Guid UserId,
    Guid TenantId,
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    TradingOrderSide Side,
    decimal Quantity,
    decimal FillPrice,
    decimal InitialMargin,
    int Leverage,
    DateTime FilledAtUtc
) : DomainEvent;
