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
    IReadOnlyList<TaxTradeRowDto> ClosedTrades
);

public sealed class ForexJournalFilter
{
    public string? Symbol { get; set; }
    public string? SetupType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
