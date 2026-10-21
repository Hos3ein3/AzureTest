using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.Forex.Enums;

namespace AzureTest.Contracts.DTOs.Forex;

public sealed record ForexStrategyDto(
    Guid Id,
    string Name,
    string Description,
    ForexStrategyType StrategyType,
    string DefaultSymbol,
    ForexTimeframe DefaultTimeframe,
    decimal RiskPercent,
    decimal StopLossPips,
    decimal TakeProfitPips,
    bool UseTrailingStop,
    decimal TrailingStopPips,
    bool BreakEvenAtPipsEnabled,
    decimal BreakEvenTriggerPips,
    string ParametersJson,
    bool IsActive,
    DateTime CreatedOn
);

public sealed record BacktestRequestDto(
    Guid? StrategyId,
    string StrategyName,
    string Symbol,
    ForexTimeframe Timeframe,
    DateTime StartDate,
    DateTime EndDate,
    decimal InitialBalance,
    decimal SpreadPips,
    decimal SlippagePips,
    decimal SwapPerDay,
    bool IsWalkForward
);

public sealed record BacktestTradeLogDto(
    DateTime EntryTime,
    DateTime ExitTime,
    string Symbol,
    ForexTradeDirection Direction,
    decimal LotSize,
    decimal EntryPrice,
    decimal ExitPrice,
    decimal Pips,
    decimal NetProfit,
    ForexExitReason Reason
);

public sealed record BacktestResultDto(
    Guid Id,
    string StrategyName,
    string Symbol,
    ForexTimeframe Timeframe,
    DateTime StartDate,
    DateTime EndDate,
    decimal InitialBalance,
    decimal FinalBalance,
    decimal NetProfit,
    decimal ReturnPercent,
    decimal MaxDrawdownPercent,
    decimal WinRatePercent,
    int TotalTrades,
    int WinningTrades,
    int LosingTrades,
    decimal ProfitFactor,
    decimal ExpectancyPips,
    decimal SharpeRatio,
    decimal SpreadPips,
    decimal SlippagePips,
    decimal TotalSwapCost,
    bool IsWalkForward,
    decimal OutOfSampleEfficiency,
    IReadOnlyList<BacktestTradeLogDto> TradeLogs,
    IReadOnlyList<decimal> EquityCurve
);

public sealed record WalkForwardResultDto(
    string InSamplePeriod,
    decimal InSampleProfitFactor,
    decimal InSampleWinRate,
    string OutOfSamplePeriod,
    decimal OutOfSampleProfitFactor,
    decimal OutOfSampleWinRate,
    decimal RobustnessRatio,
    string Verdict
);

public sealed class ForexStrategyFilter
{
    public string? Search { get; set; }
    public ForexStrategyType? StrategyType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class ForexBacktestFilter
{
    public Guid? StrategyId { get; set; }
    public string? Symbol { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
