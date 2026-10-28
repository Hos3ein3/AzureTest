using System;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingOrderDto(
    Guid Id,
    Guid AccountId,
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    TradingOrderSide Side,
    TradingOrderType OrderType,
    decimal Quantity,
    decimal? Price,
    decimal? TriggerPrice,
    int Leverage,
    decimal? StopLoss,
    decimal? TakeProfit,
    string? MultipleTakeProfitsJson,
    TimeInForce TimeInForce,
    TradingOrderStatus Status,
    DateTime PlacedAtUtc,
    DateTime? FilledAtUtc,
    DateTime? CancelledAtUtc
);
