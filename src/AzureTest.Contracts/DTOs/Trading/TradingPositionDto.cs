using System;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingPositionDto(
    Guid Id,
    Guid AccountId,
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    TradingOrderSide Side,
    decimal Quantity,
    decimal EntryPrice,
    decimal CurrentPrice,
    decimal? ClosePrice,
    int Leverage,
    decimal InitialMargin,
    decimal UnrealizedPnl,
    decimal RealizedPnl,
    decimal? StopLoss,
    decimal? TakeProfit,
    string? MultipleTakeProfitsJson,
    decimal? TrailingStopDistance,
    TradingPositionStatus Status,
    DateTime OpenedAtUtc,
    DateTime? ClosedAtUtc
);
