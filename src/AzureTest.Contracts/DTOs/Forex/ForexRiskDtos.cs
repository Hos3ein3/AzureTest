using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Forex;

public sealed record LotSizeCalculationRequestDto(
    decimal AccountBalance,
    decimal RiskPercent,
    decimal StopLossPips,
    string Symbol,
    decimal AccountLeverage = 30m
);

public sealed record LotSizeCalculationResultDto(
    decimal RecommendedLotSize,
    decimal AmountAtRisk,
    decimal PipValue,
    decimal PositionValue,
    decimal RequiredMargin,
    decimal LeverageUsed,
    bool ExceedsEsmaLimit,
    string WarningMessage
);

public sealed record CurrencyExposureDto(
    string Currency,
    decimal LongExposureLots,
    decimal ShortExposureLots,
    decimal NetExposureLots,
    decimal PercentOfEquity,
    bool IsOverexposed
);

public sealed record NewsRiskFilterDto(
    bool IsSafeToTrade,
    string StatusMessage,
    IReadOnlyList<EconomicEventDto> UpcomingHighImpactEvents,
    int MinutesToNearestEvent
);

public sealed record EsmaAfmComplianceDto(
    int MaxPermittedLeverage,
    decimal MaxMarginUsageAllowed,
    bool NegativeBalanceProtectionGuaranteed,
    bool BinaryOptionsProhibited,
    string RegulatoryFramework,
    string Country
);

public sealed record PropFirmEvaluationDto(
    string FirmName, // FTMO / FundedNext
    decimal StartingCapital,
    decimal MaxDailyLossLimitPercent,
    decimal TodayLossAmount,
    decimal TodayLossPercent,
    bool DailyLossPassed,
    decimal MaxTrailingDrawdownLimitPercent,
    decimal CurrentTrailingDrawdownPercent,
    bool MaxDrawdownPassed,
    decimal ProfitTargetPercent,
    decimal CurrentProfitPercent,
    bool ProfitTargetReached,
    int MinimumTradingDaysRequired,
    int DaysTradedCount,
    bool OverallPassed,
    string StatusSummary
);

public sealed record WeekendGapProtectionDto(
    bool IsAutoFlattenEnabled,
    System.TimeSpan FridayFlattenTimeUtc,
    bool IsMarketCloseApproaching,
    System.TimeSpan TimeUntilFridayClose,
    int OpenTradesAtRiskCount,
    decimal TotalNotionalExposureAtRisk
);

public sealed record PairSwapRateDto(
    string Symbol,
    decimal LongSwapPips,
    decimal ShortSwapPips,
    decimal DailyFinancingCostLong,
    decimal DailyFinancingCostShort,
    bool IsTripleSwapDay
);

public sealed record SwapRolloverCalendarDto(
    string TripleSwapDayOfWeek,
    System.DateTime NextTripleSwapDateUtc,
    IReadOnlyList<PairSwapRateDto> SwapRates,
    decimal EstimatedPortfolioDailySwapCost
);

public sealed record KillSwitchStatusDto(
    bool IsActive,
    System.DateTime? ActivatedAtUtc,
    string? Reason,
    int PositionsLiquidated
);

public sealed record ForexRiskOverviewDto(
    ForexAccountDto Account,
    LotSizeCalculationResultDto SampleSizing,
    IReadOnlyList<CurrencyExposureDto> CurrencyExposures,
    NewsRiskFilterDto NewsFilter,
    EsmaAfmComplianceDto Compliance,
    PropFirmEvaluationDto? PropFirm = null,
    WeekendGapProtectionDto? WeekendGap = null,
    SwapRolloverCalendarDto? SwapCalendar = null,
    KillSwitchStatusDto? KillSwitch = null
);
