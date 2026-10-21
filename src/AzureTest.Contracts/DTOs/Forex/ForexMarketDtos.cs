using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Contracts.DTOs.Forex;

public sealed record ForexQuoteDto(
    string Symbol,
    string DisplayName,
    decimal Bid,
    decimal Ask,
    decimal SpreadPips,
    decimal ChangePercent,
    decimal High24h,
    decimal Low24h,
    DateTime TimestampUtc,
    string Category
);

public sealed record ForexCandleDto(
    DateTime Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume
);

public sealed record ForexTickDto(
    string Symbol,
    decimal Bid,
    decimal Ask,
    DateTime TimestampUtc
);

public sealed record ForexSessionDto(
    ForexMarketSession Session,
    string City,
    string TimeZone,
    TimeSpan OpenUtc,
    TimeSpan CloseUtc,
    bool IsOpen,
    TimeSpan TimeUntilNextState,
    string ActiveOverlap
);

public sealed class ForexMarketFilter
{
    public string? Category { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed record MarketOverviewDto(
    IReadOnlyList<ForexQuoteDto> Quotes,
    IReadOnlyList<ForexSessionDto> Sessions,
    IReadOnlyList<ForexQuoteDto> MetalsAndIndices
);
