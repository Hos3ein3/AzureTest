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

public sealed record ForexRiskOverviewDto(
    ForexAccountDto Account,
    LotSizeCalculationResultDto SampleSizing,
    IReadOnlyList<CurrencyExposureDto> CurrencyExposures,
    NewsRiskFilterDto NewsFilter,
    EsmaAfmComplianceDto Compliance
);
