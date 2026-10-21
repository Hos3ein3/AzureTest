using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Contracts.DTOs.Forex;

public sealed record EconomicEventDto(
    Guid Id,
    string Title,
    string Country,
    string Currency,
    DateTime EventTimeUtc,
    ForexImpactLevel Impact,
    string Actual,
    string Forecast,
    string Previous,
    bool IsPending
);

public sealed record CentralBankRateDto(
    string CentralBank,
    string Country,
    string Currency,
    decimal InterestRate,
    DateTime LastChangeDate,
    string Trend
);

public sealed record ForexNewsDto(
    string Headline,
    string Source,
    string Url,
    DateTime PublishedUtc,
    string Sentiment,
    string RelatedCurrencies
);

public sealed record CotReportDto(
    string Symbol,
    string Currency,
    long NonCommercialLong,
    long NonCommercialShort,
    long NetPositions,
    long ChangeFromPreviousWeek,
    DateTime ReportDate
);

public sealed record TechnicalIndicatorsDto(
    string Symbol,
    ForexTimeframe Timeframe,
    decimal Price,
    decimal Ema9,
    decimal Ema21,
    decimal Ema50,
    decimal Ema200,
    decimal Sma20,
    decimal Sma50,
    decimal Rsi14,
    decimal MacdValue,
    decimal MacdSignal,
    decimal MacdHistogram,
    decimal Atr14,
    decimal BollingerUpper,
    decimal BollingerMiddle,
    decimal BollingerLower,
    string TrendSummary
);

public sealed record PivotPointsDto(
    string Symbol,
    decimal Pivot,
    decimal R1,
    decimal R2,
    decimal R3,
    decimal S1,
    decimal S2,
    decimal S3,
    string Method
);

public sealed record CurrencyStrengthDto(
    string Currency,
    decimal StrengthScore,
    string Status,
    decimal DailyChangePercent
);

public sealed record PairCorrelationDto(
    string PairA,
    string PairB,
    decimal CorrelationCoefficient
);

public sealed record MultiTimeframeSignalDto(
    string Symbol,
    string SignalM15,
    string SignalH1,
    string SignalH4,
    string SignalD1,
    string OverallVerdict
);

public sealed class ForexEconomicEventFilter
{
    public string? Currency { get; set; }
    public ForexImpactLevel? ImpactLevel { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public sealed record TechnicalOverviewDto(
    TechnicalIndicatorsDto Indicators,
    PivotPointsDto Pivots,
    IReadOnlyList<CurrencyStrengthDto> CurrencyStrength,
    IReadOnlyList<PairCorrelationDto> Correlations,
    MultiTimeframeSignalDto MultiTimeframe
);

public sealed record FundamentalOverviewDto(
    IReadOnlyList<EconomicEventDto> UpcomingEvents,
    IReadOnlyList<CentralBankRateDto> CentralBankRates,
    IReadOnlyList<ForexNewsDto> NewsFeed,
    IReadOnlyList<CotReportDto> CotReports
);
