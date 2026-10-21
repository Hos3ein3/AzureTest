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

public sealed record ContextMarketQuoteDto(
    string Symbol, // US10Y, DE10Y, XAU/USD, SPX, DXY, BRENT
    string Name,
    string Category, // "Bond Yield", "Metal", "Index", "Energy", "Currency Index"
    decimal Price,
    decimal ChangePercent,
    string Unit
);

public sealed record SessionSpreadLiquidityDto(
    string SessionName, // Sydney, Tokyo, London, London-NY Overlap, New York
    int LiquidityScore, // 0-100
    decimal AverageEurUsdSpreadPips,
    decimal AverageGbpUsdSpreadPips,
    decimal AverageUsdJpySpreadPips
);

public sealed record BrokerSpreadComparisonDto(
    string BrokerName,
    string Regulation,
    decimal TypicalSpreadPips,
    decimal CommissionPerLot,
    int AverageExecutionLatencyMs
);

public sealed record HourlyVolatilityDto(
    int UtcHour, // 0 to 23
    decimal AveragePipsMovement,
    string PrimarySession,
    string ActivityLevel // "Low", "Moderate", "High", "Peak Volatility"
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
    IReadOnlyList<ForexQuoteDto> MetalsAndIndices,
    IReadOnlyList<ContextMarketQuoteDto>? ContextMarkets = null,
    IReadOnlyList<SessionSpreadLiquidityDto>? SessionSpreads = null,
    IReadOnlyList<BrokerSpreadComparisonDto>? BrokerComparisons = null,
    IReadOnlyList<HourlyVolatilityDto>? HourlyVolatility = null
);
