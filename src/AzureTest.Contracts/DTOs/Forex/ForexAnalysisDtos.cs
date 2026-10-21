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
    bool IsPending,
    decimal SurpriseZScore = 0m
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
    DateTime ReportDate,
    long CommercialLong = 0,
    long CommercialShort = 0,
    decimal Percentile3Year = 50m,
    decimal ZScore = 0m,
    string ExtremePositioningStatus = "Neutral"
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

public sealed record CentralBankExpectationDto(
    string CentralBank,
    string Currency,
    decimal CurrentRatePercent,
    DateTime NextDecisionDateUtc,
    decimal PricedRateCutProbabilityPercent,
    decimal PricedRateHikeProbabilityPercent,
    decimal MarketImpliedRate12MPercent,
    int CarryTradeRank
);

public sealed record SpeechScheduleDto(
    string SpeakerName,
    string CentralBank,
    string Title,
    string Topic,
    DateTime ScheduledTimeUtc,
    ForexImpactLevel Impact,
    string LocationOrEvent
);

public sealed record CountryMacroDashboardDto(
    string CountryName,
    string Currency,
    decimal InflationRatePercent,
    decimal GdpGrowthRatePercent,
    decimal UnemploymentRatePercent,
    decimal TradeBalanceBillions,
    string PolicyStance // "Hawkish", "Dovish", "Neutral"
);

public sealed record RetailSentimentDto(
    string Symbol,
    decimal LongPercentage,
    decimal ShortPercentage,
    decimal LongShortRatio,
    int SampleTraderCount,
    string ContrarianBias // "Strongly Bullish", "Bullish", "Neutral", "Bearish", "Strongly Bearish"
);

public sealed record OptionsExpiryClusterDto(
    string Symbol,
    DateTime ExpiryDateUtc, // 10:00 AM NY cut
    decimal MaxPainStrike,
    decimal LargestCallStrike,
    decimal LargestPutStrike,
    decimal TotalOpenInterestNotionalMillions,
    decimal RiskReversal25Delta // positive = calls expensive, negative = puts expensive
);

public sealed record IntermarketYieldSpreadDto(
    string PairSymbol,
    string BaseBondYieldName, // e.g. "US 10-Year"
    decimal BaseYieldPercent,
    string QuoteBondYieldName, // e.g. "German 10-Year Bund"
    decimal QuoteYieldPercent,
    decimal SpreadBasisPoints,
    string ImpliedDirectionalBias
);

public sealed record MarketStructureDto(
    string Symbol,
    ForexTimeframe Timeframe,
    string CurrentStructure, // "Bullish (HH/HL)", "Bearish (LH/LL)", "Ranging"
    string BosDirection, // "Bullish", "Bearish", "None"
    decimal LastBosPrice,
    string ChochDirection, // "Bullish", "Bearish", "None"
    decimal LastChochPrice,
    decimal OrderBlockDemandLow,
    decimal OrderBlockDemandHigh,
    decimal OrderBlockSupplyLow,
    decimal OrderBlockSupplyHigh,
    decimal LiquiditySweepHigh,
    decimal LiquiditySweepLow
);

public sealed record FibonacciLevelsDto(
    string Symbol,
    decimal SwingHigh,
    decimal SwingLow,
    decimal Retracement236,
    decimal Retracement382,
    decimal Retracement500,
    decimal Retracement618,
    decimal Retracement786,
    decimal Extension1272,
    decimal Extension1618
);

public sealed record VwapSessionDto(
    string Symbol,
    decimal VwapPrice,
    decimal UpperBand1Sigma,
    decimal LowerBand1Sigma,
    decimal UpperBand2Sigma,
    decimal LowerBand2Sigma,
    decimal CurrentStdDevFromVwap
);

public sealed record AdrVolatilityDto(
    string Symbol,
    decimal Adr14Pips,
    decimal TodayRangePips,
    decimal PercentAdrUsed,
    decimal ImpliedVolatilityPercent,
    string MarketRegime // "Trending Expansion", "Mean-Reverting Range", "Compressed Consolidation"
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
    MultiTimeframeSignalDto MultiTimeframe,
    MarketStructureDto? MarketStructure = null,
    FibonacciLevelsDto? Fibonacci = null,
    VwapSessionDto? Vwap = null,
    AdrVolatilityDto? Volatility = null,
    RetailSentimentDto? RetailSentiment = null,
    OptionsExpiryClusterDto? OptionsExpiry = null,
    IntermarketYieldSpreadDto? YieldSpread = null
);

public sealed record FundamentalOverviewDto(
    IReadOnlyList<EconomicEventDto> UpcomingEvents,
    IReadOnlyList<CentralBankRateDto> CentralBankRates,
    IReadOnlyList<ForexNewsDto> NewsFeed,
    IReadOnlyList<CotReportDto> CotReports,
    IReadOnlyList<CentralBankExpectationDto>? CentralBankExpectations = null,
    IReadOnlyList<SpeechScheduleDto>? UpcomingSpeeches = null,
    IReadOnlyList<CountryMacroDashboardDto>? CountryDashboards = null
);
