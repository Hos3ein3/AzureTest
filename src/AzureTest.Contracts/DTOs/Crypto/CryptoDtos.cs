using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Crypto.Enums;

namespace AzureTest.Contracts.DTOs.Crypto;

// ── Market Data DTOs ─────────────────────────────────────────────────────────

public sealed record CryptoCoinDto(
    int Rank,
    string Symbol,
    string Name,
    decimal Price,
    decimal Change1h,
    decimal Change24h,
    decimal Change7d,
    decimal MarketCap,
    decimal Volume24h,
    decimal High24h,
    decimal Low24h,
    decimal CirculatingSupply);

public sealed record CryptoOrderBookEntryDto(
    decimal Price,
    decimal Quantity,
    decimal Total);

public sealed record CryptoOrderBookDto(
    string Symbol,
    DateTime Timestamp,
    IReadOnlyList<CryptoOrderBookEntryDto> Bids,
    IReadOnlyList<CryptoOrderBookEntryDto> Asks,
    decimal Spread,
    decimal SpreadPercent);

public sealed record CryptoRecentTradeDto(
    string Id,
    string Symbol,
    decimal Price,
    decimal Quantity,
    DateTime Time,
    bool IsBuyerMaker);

public sealed record CryptoCandleDto(
    DateTime Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume);

public sealed record CryptoFuturesFundingDto(
    string Symbol,
    decimal MarkPrice,
    decimal IndexPrice,
    decimal FundingRate,
    decimal EstimatedAnnualRate,
    DateTime NextFundingTimeUtc,
    decimal OpenInterest,
    decimal OpenInterestUsd,
    decimal Volume24h);

public sealed class CryptoMarketFilter
{
    public string? Search { get; set; }
    public string? Category { get; set; } = "All"; // "All", "Layer1", "DeFi", "Meme", "AI"
    public string SortBy { get; set; } = "Rank";
    public string SortDirection { get; set; } = "Asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed record CryptoMarketOverviewDto(
    IReadOnlyList<CryptoCoinDto> Coins,
    IReadOnlyList<CryptoCoinDto> TopGainers,
    IReadOnlyList<CryptoCoinDto> TopLosers,
    decimal TotalMarketCapUsd,
    decimal Total24hVolumeUsd,
    decimal BtcDominance,
    int FearAndGreedIndex,
    string SentimentLabel,
    DateTime LastUpdatedUtc);

// ── Analysis DTOs ───────────────────────────────────────────────────────────

public sealed record CryptoTechnicalIndicatorsDto(
    string Symbol,
    CryptoTimeframe Timeframe,
    decimal CurrentPrice,
    decimal Ema9,
    decimal Ema21,
    decimal Ema50,
    decimal Ema200,
    decimal Sma20,
    decimal Sma50,
    decimal Rsi14,
    string RsiSignal,
    decimal MacdLine,
    decimal SignalLine,
    decimal MacdHistogram,
    string MacdSignal,
    decimal BollingerUpper,
    decimal BollingerMiddle,
    decimal BollingerLower,
    decimal BollingerBandwidth,
    decimal Atr14,
    decimal Volume24h,
    decimal VolumeMa,
    string OverallSignal); // "Strong Buy", "Buy", "Neutral", "Sell", "Strong Sell"

public sealed record CryptoSupportResistanceDto(
    decimal Pivot,
    decimal R1,
    decimal R2,
    decimal R3,
    decimal S1,
    decimal S2,
    decimal S3,
    decimal CurrentPrice,
    string TrendDirection, // "Bullish", "Bearish", "Consolidation"
    string TrendStrength,  // "Strong", "Moderate", "Weak"
    decimal Fibonacci236 = 0m,
    decimal Fibonacci382 = 0m,
    decimal Fibonacci500 = 0m,
    decimal Fibonacci618 = 0m,
    decimal Fibonacci786 = 0m)
{
    public decimal PivotPoint => Pivot;
    public decimal Resistance1 => R1;
    public decimal Resistance2 => R2;
    public decimal Resistance3 => R3;
    public decimal Support1 => S1;
    public decimal Support2 => S2;
    public decimal Support3 => S3;
}

public sealed record CryptoNewsItemDto(
    string Title,
    string Source,
    DateTime PublishedAt,
    string Url,
    string Sentiment, // "Positive", "Negative", "Neutral"
    string Currencies);

public sealed record CryptoWhaleAlertDto(
    string TxHash,
    string Blockchain,
    string FromAddress,
    string ToAddress,
    decimal Amount,
    decimal AmountUsd,
    DateTime Timestamp,
    string AlertType); // "Exchange Inflow", "Exchange Outflow", "Whale Transfer"

public sealed record CryptoSentimentDto(
    int FearAndGreedValue,
    string FearAndGreedClassification,
    DateTime LastUpdated,
    IReadOnlyList<CryptoNewsItemDto> NewsItems,
    decimal BtcDominance);

public sealed record CryptoScreenerItemDto(
    int Rank,
    string Symbol,
    string Name,
    decimal Price,
    decimal Change24h,
    decimal Volume24h,
    decimal VolatilityPercent,
    decimal Rsi,
    string MacdStatus,
    decimal MomentumScore,
    string Recommendation);

public sealed record CryptoAnalysisDto(
    CryptoTechnicalIndicatorsDto Technical,
    CryptoSupportResistanceDto SupportResistance,
    CryptoSentimentDto Sentiment,
    IReadOnlyList<CryptoWhaleAlertDto> WhaleAlerts,
    IReadOnlyList<CryptoScreenerItemDto> ScreenerResults);

// ── Strategy & Backtest DTOs ───────────────────────────────────────────────

public sealed record CryptoStrategyDto(
    Guid Id,
    string Name,
    string Description,
    CryptoStrategyType StrategyType,
    string Symbol,
    CryptoTimeframe Timeframe,
    string ParametersJson,
    bool IsActive,
    DateTime CreatedOn);

public sealed class BacktestRequestDto
{
    public CryptoStrategyType StrategyType { get; set; } = CryptoStrategyType.TrendFollowing;
    public string Symbol { get; set; } = "BTCUSDT";
    public CryptoTimeframe Timeframe { get; set; } = CryptoTimeframe.H1;
    public decimal InitialCapital { get; set; } = 10000m;
    public decimal CommissionPercent { get; set; } = 0.075m; // 0.075% Binance standard
    public decimal SlippagePercent { get; set; } = 0.05m;
    public int DaysBack { get; set; } = 90;
    public int FastPeriod { get; set; } = 9;
    public int SlowPeriod { get; set; } = 21;
    public int RsiPeriod { get; set; } = 14;
    public int BbPeriod { get; set; } = 20;
    public decimal BbStdDev { get; set; } = 2.0m;
}

public sealed record BacktestTradeDto(
    DateTime EntryTime,
    DateTime ExitTime,
    string Side,
    decimal EntryPrice,
    decimal ExitPrice,
    decimal Quantity,
    decimal Pnl,
    decimal PnlPercent,
    string ExitReason,
    decimal FeePaid);

public sealed record EquityCurvePointDto(
    DateTime Time,
    decimal Equity,
    decimal DrawdownPercent);

public sealed record WalkForwardResultDto(
    decimal InSampleReturnPercent,
    decimal OutOfSampleReturnPercent,
    decimal OverfittingScore, // Lower is better
    string ParameterStability, // "High", "Moderate", "Unstable"
    string RobustnessRating,   // "Pass", "Caution", "Fail"
    string Notes,
    decimal InSampleSharpe = 0m,
    decimal OutOfSampleSharpe = 0m,
    decimal DegradationPercent = 0m,
    string OverfittingRisk = "Low",
    string Recommendation = "")
{
    public bool IsRobust => RobustnessRating == "Pass";
}

public sealed record BacktestResultDto(
    Guid Id,
    string StrategyName,
    CryptoStrategyType StrategyType,
    string Symbol,
    CryptoTimeframe Timeframe,
    decimal InitialCapital,
    decimal FinalCapital,
    decimal NetProfit,
    decimal ReturnPercent,
    int TotalTrades,
    int WinningTrades,
    int LosingTrades,
    decimal WinRate,
    decimal ProfitFactor,
    decimal MaxDrawdownPercent,
    decimal SharpeRatio,
    decimal TotalFeesPaid,
    IReadOnlyList<BacktestTradeDto> Trades,
    IReadOnlyList<EquityCurvePointDto> EquityCurve,
    WalkForwardResultDto WalkForward);

// ── Trading & Execution DTOs ───────────────────────────────────────────────

public sealed record CryptoOrderDto(
    Guid Id,
    Guid AccountId,
    string Symbol,
    CryptoOrderSide Side,
    CryptoOrderType OrderType,
    decimal Quantity,
    decimal RequestedPrice,
    decimal? FillPrice,
    decimal? StopPrice,
    CryptoOrderStatus Status,
    decimal Commission,
    decimal Slippage,
    string IdempotencyKey,
    string? RejectionReason,
    DateTime CreatedOn,
    DateTime? FilledAtUtc);

public sealed record CryptoPositionDto(
    Guid Id,
    Guid AccountId,
    string Symbol,
    CryptoPositionSide Side,
    decimal Quantity,
    decimal EntryPrice,
    decimal CurrentPrice,
    decimal? ClosePrice,
    int Leverage,
    decimal InitialMargin,
    decimal LiquidationPrice,
    decimal UnrealizedPnl,
    decimal RealizedPnl,
    decimal PnlPercent,
    decimal? StopLoss,
    decimal? TakeProfit,
    decimal? TrailingStopDistance,
    CryptoPositionStatus Status,
    DateTime OpenedAtUtc,
    DateTime? ClosedAtUtc);

public sealed record CryptoAccountDto(
    Guid Id,
    string Name,
    string BaseCurrency,
    CryptoTradingMode TradingMode,
    decimal CashBalance,
    decimal Equity,
    decimal MarginUsed,
    decimal FreeMargin,
    decimal DailyStartingEquity,
    CurrentDailyLossDto DailyLoss,
    decimal RiskPercentagePerTrade,
    int MaxOpenPositions,
    bool IsKillSwitchActive,
    string? KillSwitchReason);

public sealed record CurrentDailyLossDto(
    decimal Amount,
    decimal Percent,
    decimal LimitPercent,
    bool IsBreached);

public sealed record TradeTerminalDashboardDto(
    CryptoAccountDto Account,
    IReadOnlyList<CryptoPositionDto> OpenPositions,
    IReadOnlyList<CryptoOrderDto> RecentOrders,
    IReadOnlyList<CryptoCoinDto> ActiveCoins,
    string SelectedSymbol,
    decimal CurrentPrice,
    CryptoOrderBookDto Depth,
    IReadOnlyList<CryptoRecentTradeDto> RecentTrades,
    CryptoFuturesFundingDto? FuturesData);

// ── Risk & Position Sizing DTOs ────────────────────────────────────────────

public sealed record PositionSizeCalculatorDto(
    decimal AccountEquity,
    decimal RiskPercent,
    decimal RiskAmount,
    decimal EntryPrice,
    decimal StopLossPrice,
    decimal RecommendedQuantity,
    decimal TotalPositionValue,
    int RecommendedLeverage,
    decimal LiquidationPrice,
    decimal DistanceToLiquidationPercent);

public sealed record CryptoRiskOverviewDto(
    CryptoAccountDto Account,
    decimal TotalRiskExposureUsd,
    decimal MarginLevelPercent,
    int OpenPositionsCount,
    int MaxAllowedPositions,
    bool KillSwitchActive,
    PositionSizeCalculatorDto PositionSizingGuide,
    IReadOnlyList<string> RiskAlerts,
    string DutchAfmWarningNote);

// ── Portfolio, Tax & Alerts DTOs ───────────────────────────────────────────

public sealed record CryptoJournalEntryDto(
    Guid Id,
    Guid? PositionId,
    string Symbol,
    string StrategyTag,
    string EmotionTag,
    int Rating,
    string Notes,
    string LessonsLearned,
    DateTime CreatedOn);

public sealed record CryptoAlertDto(
    Guid Id,
    string Symbol,
    CryptoAlertType AlertType,
    decimal TargetValue,
    string Message,
    string Channel,
    bool IsActive,
    bool IsTriggered,
    DateTime? TriggeredAtUtc,
    DateTime CreatedOn);

public sealed record CryptoApiSettingsDto(
    CryptoExchange Exchange,
    string ApiKeyMasked,
    bool IsTestnet,
    bool IsActive,
    bool IsConnected,
    DateTime? LastTestedAtUtc,
    string? ConnectionStatusMessage);

public sealed record CryptoTaxItemDto(
    DateTime Date,
    string Symbol,
    string Action,
    decimal Quantity,
    decimal PriceEur,
    decimal TotalValueEur,
    decimal RealizedGainLossEur);

public sealed record CryptoTaxReportDto(
    int TaxYear,
    decimal TotalRealizedPnlEur,
    int TotalTradesCount,
    decimal TotalVolumeEur,
    decimal EstimatedBox3ValuationJan1stEur,
    string DutchTaxBox3Guidance,
    IReadOnlyList<CryptoTaxItemDto> Items);

public sealed record CryptoTaxSummaryDto(
    decimal TotalRealizedGainLoss,
    decimal TotalVolumeTraded,
    int CompletedTradesCount,
    decimal Jan1stReferenceBalance,
    string TaxJurisdiction);
