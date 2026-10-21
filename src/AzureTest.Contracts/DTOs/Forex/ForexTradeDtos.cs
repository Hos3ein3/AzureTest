using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Contracts.DTOs.Forex;

public sealed record ForexAccountDto(
    Guid Id,
    string Name,
    string BaseCurrency,
    decimal Balance,
    decimal Equity,
    decimal UsedMargin,
    decimal FreeMargin,
    decimal MarginLevelPercent,
    int Leverage,
    bool IsDemo,
    decimal PeakEquity,
    decimal DailyStartingBalance,
    decimal DailyPnl,
    decimal DailyDrawdownPercent
);

public sealed record ForexOrderDto(
    Guid Id,
    Guid AccountId,
    string Symbol,
    ForexTradeDirection Direction,
    ForexOrderType OrderType,
    decimal LotSize,
    decimal RequestedPrice,
    decimal? FillPrice,
    decimal? StopLoss,
    decimal? TakeProfit,
    decimal SlippagePips,
    ForexOrderStatus Status,
    string IdempotencyKey,
    string? RejectionReason,
    DateTime CreatedOn,
    DateTime? FilledAtUtc
);

public sealed record ForexTradeDto(
    Guid Id,
    Guid AccountId,
    Guid? OrderId,
    string Symbol,
    ForexTradeDirection Direction,
    decimal LotSize,
    decimal OpenPrice,
    decimal? ClosePrice,
    decimal? StopLoss,
    decimal? TakeProfit,
    decimal TrailingStopPips,
    bool BreakEvenTriggered,
    decimal MarginRequired,
    decimal SwapCost,
    decimal Commission,
    decimal GrossProfit,
    decimal NetProfit,
    decimal Pips,
    ForexTradeStatus Status,
    ForexExitReason ExitReason,
    DateTime OpenTimeUtc,
    DateTime? CloseTimeUtc,
    string? Notes,
    string? ScreenshotUrl,
    decimal CurrentPrice,
    decimal UnrealizedPnl,
    decimal UnrealizedPips
);

public sealed record TradeTerminalDashboardDto(
    ForexAccountDto Account,
    IReadOnlyList<ForexTradeDto> OpenTrades,
    IReadOnlyList<ForexOrderDto> PendingOrders,
    IReadOnlyList<ForexTradeDto> RecentClosedTrades,
    IReadOnlyList<ForexQuoteDto> LiveWatchlist
);

public sealed class ForexTradeFilter
{
    public ForexTradeStatus? Status { get; set; }
    public string? Symbol { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class ForexOrderFilter
{
    public ForexOrderStatus? Status { get; set; }
    public string? Symbol { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
