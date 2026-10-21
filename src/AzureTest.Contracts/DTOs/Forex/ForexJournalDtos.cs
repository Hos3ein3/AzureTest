using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Contracts.DTOs.Forex;

public sealed record ForexJournalEntryDto(
    Guid Id,
    Guid? TradeId,
    string Symbol,
    DateTime TradeDateUtc,
    string MarketSession,
    string SetupType,
    string Emotions,
    string Mistakes,
    string LessonsLearned,
    string DetailedNotes,
    string? ScreenshotUrl,
    int ExecutionRating,
    bool FollowedPlan,
    DateTime CreatedOn
);

public sealed record TaxTradeRowDto(
    DateTime OpenDate,
    DateTime CloseDate,
    string Symbol,
    ForexTradeDirection Direction,
    decimal LotSize,
    decimal OpenPrice,
    decimal ClosePrice,
    decimal NetProfit,
    decimal FeesAndSwaps
);

public sealed record TaxReportSummaryDto(
    int TaxYear,
    decimal TotalGrossProfits,
    decimal TotalLosses,
    decimal NetRealizedGainLoss,
    decimal TotalFeesAndSwaps,
    int TotalTradesClosed,
    IReadOnlyList<TaxTradeRowDto> ClosedTrades,
    decimal Box3DeemedYieldEstimate = 0m,
    string NetherlandsTaxNotes = "Box 3 capital assets threshold rules apply. Professional active day trading may fall under Box 1 freelance income."
);

public sealed record RMultipleDistributionBucketDto(
    string RRangeLabel,
    int TradeCount,
    decimal PercentageOfTotal,
    decimal TotalNetProfit
);

public sealed record PerformanceByDimensionDto(
    string Category,
    string Name,
    int TotalTrades,
    decimal WinRatePercent,
    decimal ProfitFactor,
    decimal NetProfit,
    decimal ExpectancyPips
);

public sealed record JournalAnalyticsDashboardDto(
    decimal TotalNetProfit,
    decimal ProfitFactor,
    decimal WinRatePercent,
    decimal ExpectancyPips,
    decimal ExpectancyDollars,
    decimal SharpeRatio,
    decimal SortinoRatio,
    decimal MaxDrawdownPercent,
    decimal AverageWinAmount,
    decimal AverageLossAmount,
    decimal WinLossRatio,
    IReadOnlyList<RMultipleDistributionBucketDto> RMultipleDistribution,
    IReadOnlyList<PerformanceByDimensionDto> PerformanceBySession,
    IReadOnlyList<PerformanceByDimensionDto> PerformanceByWeekday,
    IReadOnlyList<PerformanceByDimensionDto> PerformanceByPair,
    IReadOnlyList<PerformanceByDimensionDto> PerformanceByStrategy
);

public sealed class ForexJournalFilter
{
    public string? Symbol { get; set; }
    public string? SetupType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
