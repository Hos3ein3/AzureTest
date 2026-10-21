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

public sealed record CryptoLongShortRatioDto(
    string Symbol,
    decimal LongAccountPercent,
    decimal ShortAccountPercent,
    decimal LongShortRatio,
    decimal TopTradersLongPercent,
    decimal TopTradersShortPercent,
    decimal TopTradersRatio,
    string SentimentBias);

public sealed record CryptoLiquidationClusterDto(
    decimal PriceLevel,
    decimal EstimatedLiquidationUsd,
    string LeverageLevel,
    string Side,
    decimal DistancePercent);

public sealed record CryptoLiquidationOrderDto(
    string Id,
    string Symbol,
    string Side,
    decimal Price,
    decimal Quantity,
    decimal TotalUsd,
    DateTime Time);

public sealed record CryptoLiquidationMapDto(
    string Symbol,
    decimal CurrentPrice,
    decimal Total24hLiquidationsUsd,
    decimal LongLiquidations24hUsd,
    decimal ShortLiquidations24hUsd,
    IReadOnlyList<CryptoLiquidationClusterDto> Clusters,
    IReadOnlyList<CryptoLiquidationOrderDto> LiveFeed);

public sealed record CryptoOptionsMarketDto(
    string UnderlyingSymbol,
    decimal ImpliedVolatilityDvol,
    decimal HistoricalVolatility30d,
    decimal MaxPainStrike,
    decimal CurrentSpotPrice,
    decimal PutCallRatioOi,
    decimal PutCallRatioVolume24h,
    decimal Skew25Delta,
    decimal TotalOpenInterestUsd,
    decimal Total24hVolumeUsd);

public sealed record CryptoFuturesContractDto(
    string ContractName,
    DateTime ExpiryDateUtc,
    int DaysToExpiry,
    decimal FuturesPrice,
    decimal BasisUsd,
    decimal BasisPercent,
    decimal AnnualizedYieldPercent);

public sealed record CryptoFuturesCurveDto(
    string Symbol,
    decimal SpotPrice,
    string Regime,
    decimal AverageAnnualizedBasisPercent,
    IReadOnlyList<CryptoFuturesContractDto> Contracts);

public sealed record CryptoDerivativesHubDto(
    string Symbol,
    CryptoFuturesFundingDto Funding,
    CryptoLongShortRatioDto LongShortRatio,
    CryptoLiquidationMapDto LiquidationMap,
    CryptoOptionsMarketDto Options,
    CryptoFuturesCurveDto Curve,
    DateTime LastUpdatedUtc);


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

// ── Advanced Market Data & Order Flow DTOs ──────────────────────────────────

public sealed record CryptoFootprintLevelDto(
    decimal Price,
    decimal BidVolume,
    decimal AskVolume,
    decimal Delta,
    bool HasImbalance);

public sealed record CryptoFootprintBarDto(
    DateTime Time,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume,
    decimal TotalDelta,
    decimal CumulativeDelta,
    decimal PocPrice,
    IReadOnlyList<CryptoFootprintLevelDto> Levels);

public sealed record CryptoCvdPointDto(
    DateTime Time,
    decimal Price,
    decimal VolumeDelta,
    decimal CumulativeDelta,
    string Divergence);

public sealed record CryptoLiquidityWallDto(
    decimal Price,
    decimal Quantity,
    decimal TotalUsd,
    string Side,
    decimal DistancePercent,
    decimal PersistencePercent,
    bool IsAuthentic);

public sealed record CryptoOrderBookHeatmapDto(
    string Symbol,
    DateTime Timestamp,
    decimal CurrentPrice,
    IReadOnlyList<CryptoLiquidityWallDto> Walls,
    IReadOnlyList<CryptoOrderBookEntryDto> BidDistribution,
    IReadOnlyList<CryptoOrderBookEntryDto> AskDistribution);

public sealed record CryptoCrossExchangeRateDto(
    string Exchange,
    decimal Price,
    decimal SpreadPercent,
    DateTime LastUpdate);

public sealed record CryptoCrossExchangePremiumDto(
    string Symbol,
    decimal BinancePrice,
    decimal CoinbasePrice,
    decimal KrakenPrice,
    decimal BybitPrice,
    decimal CoinbasePremiumUsd,
    decimal CoinbasePremiumPercent,
    decimal BestBid,
    string BestBidExchange,
    decimal BestAsk,
    string BestAskExchange,
    decimal MaxArbitragePercent,
    IReadOnlyList<CryptoCrossExchangeRateDto> Rates);

public sealed record CryptoStablecoinDto(
    string Symbol,
    string Name,
    decimal CurrentPrice,
    decimal PegDeviationPercent,
    decimal MarketCapUsd,
    decimal Supply24hChangeUsd,
    string PegStatus,
    DateTime LastUpdatedUtc);

public sealed record CryptoMarketDataHubDto(
    string Symbol,
    CryptoTimeframe Timeframe,
    decimal CurrentPrice,
    IReadOnlyList<CryptoFootprintBarDto> FootprintBars,
    IReadOnlyList<CryptoCvdPointDto> CvdPoints,
    CryptoOrderBookHeatmapDto Heatmap,
    CryptoCrossExchangePremiumDto Premiums,
    IReadOnlyList<CryptoStablecoinDto> Stablecoins,
    DateTime LastUpdatedUtc);

// ── Feature 3: On-Chain and Flow DTOs ────────────────────────────────────────

public sealed record CryptoExchangeFlowDto(
    string ExchangeName,
    decimal InflowUsd24h,
    decimal OutflowUsd24h,
    decimal NetFlowUsd24h,
    decimal ReserveBtc,
    decimal ReserveChange24hPercent,
    string FlowSignal);

public sealed record CryptoWhaleTransactionDto(
    string TxHash,
    string Blockchain,
    string AssetSymbol,
    decimal Amount,
    decimal AmountUsd,
    string FromAddress,
    string FromLabel,
    string ToAddress,
    string ToLabel,
    string FlowType,
    DateTime TimestampUtc);

public sealed record CryptoStablecoinMintBurnDto(
    string Stablecoin,
    string Action,
    decimal Amount,
    string Blockchain,
    string RecipientOrSender,
    DateTime TimestampUtc,
    string MarketImpact);

public sealed record CryptoStablecoinSupplyFlowDto(
    string Stablecoin,
    decimal CirculatingSupplyUsd,
    decimal NetSupplyChange24hUsd,
    decimal NetSupplyChange7dUsd,
    decimal MarketDominancePercent,
    string Trend);

public sealed record CryptoTokenUnlockDto(
    string Symbol,
    string ProjectName,
    DateTime UnlockDateUtc,
    int DaysUntilUnlock,
    decimal UnlockAmount,
    decimal UnlockValueUsd,
    decimal PercentOfCirculatingSupply,
    string UnlockCategory,
    string ShockRiskLevel);

public sealed record CryptoDexPoolDto(
    string Pair,
    string DexName,
    string Chain,
    decimal PriceUsd,
    decimal PriceChange24hPercent,
    decimal LiquidityTvlUsd,
    decimal Volume24hUsd,
    decimal VolumeToTvlRatio,
    decimal SafetyScore,
    bool IsLiquidityLocked,
    bool IsMintRevoked,
    decimal Top10HoldersPercent);

public sealed record CryptoSmartMoneyWalletDto(
    string WalletAddress,
    string EntityLabel,
    decimal Realized30dPnlUsd,
    decimal WinRatePercent,
    decimal EstimatedNetWorthUsd,
    string PrimaryAssets,
    string CurrentStrategy,
    int CopyAlertSubscribers,
    bool IsCopyAlertActive);

public sealed record CryptoSmartMoneyActivityDto(
    string TxHash,
    string EntityLabel,
    string Action,
    string AssetSymbol,
    decimal Amount,
    decimal ValueUsd,
    decimal ExecutionPrice,
    string Venue,
    DateTime TimestampUtc,
    string CopySignal);

public sealed record CryptoOnChainHubDto(
    IReadOnlyList<CryptoExchangeFlowDto> ExchangeFlows,
    decimal TotalNetExchangeFlow24hUsd,
    IReadOnlyList<CryptoWhaleTransactionDto> WhaleTransactions,
    IReadOnlyList<CryptoStablecoinSupplyFlowDto> StablecoinSupplies,
    IReadOnlyList<CryptoStablecoinMintBurnDto> RecentMintsAndBurns,
    IReadOnlyList<CryptoTokenUnlockDto> UpcomingUnlocks,
    IReadOnlyList<CryptoDexPoolDto> DexScreening,
    IReadOnlyList<CryptoSmartMoneyWalletDto> SmartMoneyWallets,
    IReadOnlyList<CryptoSmartMoneyActivityDto> SmartMoneyActivityFeed,
    DateTime LastUpdatedUtc);

// ── 3.1 Exchange Net Flow Analysis DTOs ──────────────────────────────────────

public sealed record CryptoExchangeDetailFlowDto(
    string ExchangeName,
    decimal Inflow1hUsd,
    decimal Outflow1hUsd,
    decimal NetFlow1hUsd,
    decimal Inflow24hUsd,
    decimal Outflow24hUsd,
    decimal NetFlow24hUsd,
    decimal NetFlow7dUsd,
    decimal NetFlow30dUsd,
    decimal BtcReserve,
    decimal EthReserve,
    decimal StablecoinReserveUsd,
    decimal ReserveChange24hPercent,
    decimal MeanInflowVolumeBtc,
    string FlowSignal,
    string DominantDirection);

public sealed record CryptoExchangeFlowTrendPointDto(
    DateTime Time,
    decimal NetFlowUsd,
    decimal CumulativeFlowUsd,
    decimal BtcPrice);

public sealed record CryptoMinerToExchangeFlowDto(
    string PoolName,
    decimal Transferred24hBtc,
    decimal TransferredUsd,
    decimal MinerOutflowMultiple,
    string CapitulationStatus);

public sealed record CryptoExchangeFlowAnalysisHubDto(
    IReadOnlyList<CryptoExchangeDetailFlowDto> Exchanges,
    decimal TotalNetFlow1hUsd,
    decimal TotalNetFlow24hUsd,
    decimal TotalNetFlow7dUsd,
    decimal TotalNetFlow30dUsd,
    IReadOnlyList<CryptoExchangeFlowTrendPointDto> HistoricalTrend,
    IReadOnlyList<CryptoMinerToExchangeFlowDto> MinerFlows,
    decimal SpotVsDerivativesRatio,
    string MacroFlowVerdict,
    DateTime LastUpdatedUtc);

// ── 3.2 Token Unlocks Calendar & Dilution Pressure DTOs ──────────────────────

public sealed record CryptoUnlockAllocationShareDto(
    string Category,
    decimal TokenAmount,
    decimal ValueUsd,
    decimal Percentage);

public sealed record CryptoTokenUnlockDetailDto(
    string Symbol,
    string ProjectName,
    DateTime NextUnlockDateUtc,
    int DaysUntilUnlock,
    decimal UnlockTokens,
    decimal UnlockValueUsd,
    decimal CurrentPriceUsd,
    decimal CirculatingSupply,
    decimal TotalSupply,
    decimal DilutionPercentageCirculating,
    decimal DilutionPercentageTotalFdv,
    string UnlockType,
    IReadOnlyList<CryptoUnlockAllocationShareDto> AllocationBreakdown,
    decimal Historical7dPostUnlockReturnPercent,
    string HedgingRecommendation,
    string DilutionShockRating);

public sealed record CryptoTokenUnlocksHubDto(
    IReadOnlyList<CryptoTokenUnlockDetailDto> Unlocks,
    decimal TotalUpcoming30dUnlockValueUsd,
    string HighestDilutionAssetSymbol,
    int HighRiskUnlocksCount,
    DateTime LastUpdatedUtc);

// ── 3.3 DEX Liquidity & New Pool Rugpull DTOs ───────────────────────────────

public sealed record CryptoRugpullAuditDto(
    string TokenName,
    string TokenSymbol,
    string ContractAddress,
    string Chain,
    decimal SafetyScore,
    string Verdict,
    bool IsMintRevoked,
    bool HasBlacklistFunction,
    bool HasPausableTransfers,
    decimal BuyTaxPercent,
    decimal SellTaxPercent,
    bool IsTaxModifiable,
    bool IsLiquidityLocked,
    decimal LiquidityLockedPercent,
    string LockDurationText,
    decimal Top10HoldersPercent,
    int TotalHolders,
    string DeployerWalletAddress,
    int DeployerPreviousProjectsCount,
    int DeployerPastRugpullsCount);

public sealed record CryptoNewDexPoolDto(
    string Pair,
    string BaseToken,
    string QuoteToken,
    string Dex,
    string Chain,
    decimal PriceUsd,
    decimal InitialLiquidityUsd,
    decimal CurrentLiquidityUsd,
    decimal Volume24hUsd,
    decimal VolumeToTvl,
    int UniqueBuyers24h,
    int UniqueSellers24h,
    decimal BuySellRatio,
    DateTime CreatedAtUtc,
    int AgeMinutes,
    CryptoRugpullAuditDto Audit);

public sealed record CryptoDexScreenerHubDto(
    IReadOnlyList<CryptoNewDexPoolDto> Pools,
    IReadOnlyList<CryptoNewDexPoolDto> NewLaunchesRadar,
    int TotalPoolsAudited,
    int HoneypotsBlockedCount,
    decimal TotalDexVolume24hUsd,
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
    IReadOnlyList<CryptoScreenerItemDto> ScreenerResults,
    CryptoMarketStructureDto? MarketStructure = null,
    CryptoVolumeProfileHubDto? VolumeProfile = null,
    CryptoMarketRegimeDto? MarketRegime = null);

// ── Feature 4: Analysis DTOs (4.1 SMC, 4.2 VPVR & AVWAP, 4.3 Market Regime) ──

public sealed record CryptoOrderBlockDto(
    string Type,
    decimal TopPrice,
    decimal BottomPrice,
    decimal MeanThreshold,
    DateTime FormedAtUtc,
    bool IsMitigated,
    decimal MitigationPrice,
    decimal StrengthScore);

public sealed record CryptoFairValueGapDto(
    string Type,
    decimal TopPrice,
    decimal BottomPrice,
    decimal ConsequentEncroachment,
    DateTime FormedAtUtc,
    bool IsFilled,
    decimal PercentFilled);

public sealed record CryptoLiquidityPoolDto(
    string Type,
    decimal PriceLevel,
    string Status,
    DateTime? SweptAtUtc,
    decimal EstimatedVolumeUsd);

public sealed record CryptoMarketStructureDto(
    string Symbol,
    CryptoTimeframe Timeframe,
    string CurrentTrend,
    string LastBreakType,
    decimal LastBreakPrice,
    DateTime LastBreakTimeUtc,
    decimal RecentSwingHigh,
    decimal RecentSwingLow,
    decimal EquilibriumPrice,
    string CurrentZone,
    IReadOnlyList<CryptoOrderBlockDto> OrderBlocks,
    IReadOnlyList<CryptoFairValueGapDto> FairValueGaps,
    IReadOnlyList<CryptoLiquidityPoolDto> LiquidityPools,
    string SmcTradeBias,
    DateTime LastUpdatedUtc);

public sealed record CryptoVolumeProfileBucketDto(
    decimal PriceLevel,
    decimal BuyVolume,
    decimal SellVolume,
    decimal TotalVolume,
    bool IsPoc,
    bool IsValueArea,
    string NodeClassification);

public sealed record CryptoAnchoredVwapDto(
    string AnchorName,
    DateTime AnchorDateUtc,
    decimal VwapPrice,
    decimal UpperBand1Sigma,
    decimal LowerBand1Sigma,
    decimal UpperBand2Sigma,
    decimal LowerBand2Sigma,
    decimal CurrentDeviationPercent,
    string PositionVerdict);

public sealed record CryptoVolumeProfileHubDto(
    string Symbol,
    CryptoTimeframe Timeframe,
    decimal CurrentPrice,
    decimal PocPrice,
    decimal ValueAreaHigh,
    decimal ValueAreaLow,
    decimal TotalVisibleVolumeUsd,
    decimal ValueAreaVolumePercent,
    IReadOnlyList<CryptoVolumeProfileBucketDto> Buckets,
    IReadOnlyList<CryptoAnchoredVwapDto> AnchoredVwaps,
    string VolumeProfileSignal,
    DateTime LastUpdatedUtc);

public sealed record CryptoMarketRegimeDto(
    string Symbol,
    string RegimeName,
    decimal RegimeConfidencePercent,
    decimal AdxValue,
    decimal AtrPercent,
    decimal BollingerBandwidth,
    bool IsBollingerSqueeze,
    decimal HurstExponent,
    string PrimaryMarketDriver,
    string OptimalStrategy,
    string TacticalGuidance,
    DateTime LastUpdatedUtc);

public sealed record CryptoAnalysisHubDto(
    string Symbol,
    CryptoTimeframe Timeframe,
    decimal CurrentPrice,
    CryptoTechnicalIndicatorsDto Technical,
    CryptoSupportResistanceDto SupportResistance,
    CryptoMarketStructureDto MarketStructure,
    CryptoVolumeProfileHubDto VolumeProfile,
    CryptoMarketRegimeDto MarketRegime,
    DateTime LastUpdatedUtc);

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
    public decimal CommissionPercent { get; set; } = 0.05m; // Taker standard
    public decimal MakerFeePercent { get; set; } = 0.02m;
    public decimal SlippagePercent { get; set; } = 0.04m;
    public int DaysBack { get; set; } = 90;
    public int FastPeriod { get; set; } = 9;
    public int SlowPeriod { get; set; } = 21;
    public int RsiPeriod { get; set; } = 14;
    public int BbPeriod { get; set; } = 20;
    public decimal BbStdDev { get; set; } = 2.0m;
    public decimal Leverage { get; set; } = 1.0m; // 1x to 20x
    public string FeeTier { get; set; } = "VIP 0 (0.05% Taker / 0.02% Maker)";
    public decimal FundingRate8hPercent { get; set; } = 0.01m; // 0.01% per 8h
    public bool EnableDynamicSlippage { get; set; } = true;
    public bool EnableLiquidationSimulation { get; set; } = true;
    public int MonteCarloIterations { get; set; } = 10000;
    public int WalkForwardWindowsCount { get; set; } = 4;
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
    decimal FeePaid,
    decimal FundingFeePaid = 0m,
    decimal SlippageDragUsd = 0m);

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

// ── Feature 5: Strategy Lab, WFA & Monte Carlo Engine DTOs ──────────────────

public sealed record CryptoBacktestFrictionSummaryDto(
    string FeeTier,
    decimal MakerFeePercent,
    decimal TakerFeePercent,
    decimal TotalCommissionPaid,
    decimal TotalSlippageCost,
    decimal TotalFundingFeesPaid,
    decimal TotalFrictionCost,
    decimal FrictionDragPercentOfReturn,
    bool LiquidationOccurred,
    decimal LiquidationThresholdPrice,
    decimal MaintenanceMarginPercent);

public sealed record CryptoWalkForwardWindowDto(
    int WindowIndex,
    DateTime InSampleStart,
    DateTime InSampleEnd,
    DateTime OutOfSampleStart,
    DateTime OutOfSampleEnd,
    decimal InSampleReturnPercent,
    decimal OutOfSampleReturnPercent,
    decimal InSampleSharpe,
    decimal OutOfSampleSharpe,
    decimal WalkForwardEfficiencyPercent,
    string Status);

public sealed record CryptoOverfittingShieldDto(
    decimal DeflatedSharpeRatio,
    decimal ProbabilityOfOverfittingPercent,
    int MinTrackRecordLengthDays,
    decimal Skewness,
    decimal Kurtosis,
    int ParameterCombinationsTested,
    string OverfittingRiskLevel,
    string ShieldVerdict);

public sealed record CryptoMonteCarloPercentileDto(
    int Percentile,
    decimal ProjectedCapital,
    decimal ReturnPercent,
    decimal MaxDrawdownPercent);

public sealed record CryptoMonteCarloSimulationDto(
    int TotalSimulations,
    decimal ProbabilityOfProfitPercent,
    decimal RiskOfRuin20PercentDrawdown,
    decimal RiskOfRuin30PercentDrawdown,
    decimal RiskOfRuin50PercentDrawdown,
    decimal TotalRuinProbabilityPercent,
    decimal MedianFinalCapital,
    decimal MedianMaxDrawdownPercent,
    decimal WorstCaseDrawdownPercent,
    IReadOnlyList<CryptoMonteCarloPercentileDto> Percentiles,
    IReadOnlyList<decimal> SampledEquityTrajectories)
{
    public IReadOnlyList<CryptoMonteCarloPercentileDto> ConfidenceBands => Percentiles;
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
    WalkForwardResultDto WalkForward,
    CryptoBacktestFrictionSummaryDto? FrictionSummary = null,
    IReadOnlyList<CryptoWalkForwardWindowDto>? WalkForwardWindows = null,
    CryptoOverfittingShieldDto? OverfittingShield = null,
    CryptoMonteCarloSimulationDto? MonteCarlo = null);

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
