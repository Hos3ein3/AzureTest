using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Contracts.DTOs.Trading;

public sealed record TradingHistorySummaryDto(
    int TotalTrades,
    int WinningTrades,
    int LosingTrades,
    int BreakEvenTrades,
    decimal WinRatePercentage,
    decimal TotalRealizedProfit,
    decimal TotalRealizedLoss,
    decimal NetRealizedPnl,
    decimal ProfitFactor,
    decimal AverageWin,
    decimal AverageLoss,
    decimal PayoffRatio,
    decimal LargestWin,
    decimal LargestLoss,
    decimal TotalVolumeNotional
);

public sealed record TradingHistoryItemDto(
    Guid Id,
    Guid AccountId,
    string Symbol,
    string AssetName,
    AssetClass AssetClass,
    TradingOrderSide Side,
    decimal Quantity,                  // Units
    decimal EntryPrice,
    decimal? ClosePrice,               // Price that closed the trade (exit price / last price)
    decimal CurrentPrice,
    int Leverage,                      // Selected leverage
    decimal InitialMargin,             // Selected margin / selected balance
    decimal WholeAccountBalance,       // Whole account balance
    decimal BalancePercentage,         // % of balance allocated (InitialMargin / WholeAccountBalance * 100)
    decimal NotionalValue,             // Quantity * EntryPrice
    decimal? StopLoss,                 // Level of SL
    decimal? TakeProfit,               // Level of TP
    string? MultipleTakeProfitsJson,   // Multi-level TPs
    decimal? TrailingStopDistance,
    decimal RealizedPnl,
    decimal RoiPercentage,             // (RealizedPnl / InitialMargin) * 100
    TradingPositionStatus Status,      // Closed, Liquidated
    string OutcomeBadge,               // "WIN", "LOSS", "BREAK-EVEN", "LIQUIDATED"
    string CloseReasonBadge,           // "TP HIT", "SL HIT", "LIQUIDATION", "MANUAL EXIT"
    DateTime OpenedAtUtc,
    DateTime? ClosedAtUtc,
    TimeSpan? Duration
);

public sealed record TradingHistoryDto(
    TradingAccountDto Account,
    TradingHistorySummaryDto Summary,
    IReadOnlyList<TradingHistoryItemDto> Trades,
    string? SearchFilter = null,
    TradingOrderSide? SideFilter = null,
    AssetClass? AssetClassFilter = null,
    string? OutcomeFilter = null
);
