using System;
using AzureTest.Domain.Entities.Forex.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexBacktest : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int StrategyNameMaxLength = 100;
        public const int SymbolMaxLength = 20;
        public const int NotesMaxLength = 2000;
        public const int JsonReportMaxLength = 100_000;
        public const int DecimalPrecision = 18;
        public const int DecimalScale = 2;
        public const int RatioPrecision = 10;
        public const int RatioScale = 4;
    }

    public Guid? StrategyId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string StrategyName { get; private set; } = string.Empty;
    public string Symbol { get; private set; } = "EUR/USD";
    public ForexTimeframe Timeframe { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }
    public decimal InitialBalance { get; private set; }
    public decimal FinalBalance { get; private set; }
    public decimal NetProfit { get; private set; }
    public decimal ReturnPercent { get; private set; }
    public decimal MaxDrawdownPercent { get; private set; }
    public decimal WinRatePercent { get; private set; }
    public int TotalTrades { get; private set; }
    public int WinningTrades { get; private set; }
    public int LosingTrades { get; private set; }
    public decimal ProfitFactor { get; private set; }
    public decimal ExpectancyPips { get; private set; }
    public decimal SharpeRatio { get; private set; }
    public decimal SpreadPips { get; private set; }
    public decimal SlippagePips { get; private set; }
    public decimal TotalSwapCost { get; private set; }
    public bool IsWalkForward { get; private set; }
    public decimal OutOfSampleEfficiency { get; private set; }
    public string TradeLogsJson { get; private set; } = "[]";
    public string? Notes { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private ForexBacktest() { }

    private ForexBacktest(
        Guid? strategyId,
        Guid userId,
        string strategyName,
        string symbol,
        ForexTimeframe timeframe,
        DateTime startDateUtc,
        DateTime endDateUtc,
        decimal initialBalance,
        decimal finalBalance,
        decimal maxDrawdownPercent,
        decimal winRatePercent,
        int totalTrades,
        int winningTrades,
        int losingTrades,
        decimal profitFactor,
        decimal expectancyPips,
        decimal sharpeRatio,
        decimal spreadPips,
        decimal slippagePips,
        decimal totalSwapCost,
        bool isWalkForward,
        decimal outOfSampleEfficiency,
        string tradeLogsJson,
        string? notes,
        Guid? tenantId = null)
    {
        StrategyId = strategyId;
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        StrategyName = strategyName.Trim();
        Symbol = symbol.ToUpperInvariant().Trim();
        Timeframe = timeframe;
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        InitialBalance = initialBalance;
        FinalBalance = finalBalance;
        NetProfit = finalBalance - initialBalance;
        ReturnPercent = initialBalance > 0m ? Math.Round((NetProfit / initialBalance) * 100m, 2) : 0m;
        MaxDrawdownPercent = maxDrawdownPercent;
        WinRatePercent = winRatePercent;
        TotalTrades = totalTrades;
        WinningTrades = winningTrades;
        LosingTrades = losingTrades;
        ProfitFactor = profitFactor;
        ExpectancyPips = expectancyPips;
        SharpeRatio = sharpeRatio;
        SpreadPips = spreadPips;
        SlippagePips = slippagePips;
        TotalSwapCost = totalSwapCost;
        IsWalkForward = isWalkForward;
        OutOfSampleEfficiency = outOfSampleEfficiency;
        TradeLogsJson = tradeLogsJson;
        Notes = notes;
    }

    public static ForexBacktest Create(
        Guid? strategyId,
        Guid userId,
        string strategyName,
        string symbol,
        ForexTimeframe timeframe,
        DateTime startDateUtc,
        DateTime endDateUtc,
        decimal initialBalance,
        decimal finalBalance,
        decimal maxDrawdownPercent,
        decimal winRatePercent,
        int totalTrades,
        int winningTrades,
        int losingTrades,
        decimal profitFactor,
        decimal expectancyPips,
        decimal sharpeRatio,
        decimal spreadPips,
        decimal slippagePips,
        decimal totalSwapCost,
        bool isWalkForward,
        decimal outOfSampleEfficiency,
        string tradeLogsJson,
        string? notes = null,
        Guid? tenantId = null)
    {
        return new ForexBacktest(
            strategyId, userId, strategyName, symbol, timeframe, startDateUtc, endDateUtc,
            initialBalance, finalBalance, maxDrawdownPercent, winRatePercent, totalTrades,
            winningTrades, losingTrades, profitFactor, expectancyPips, sharpeRatio,
            spreadPips, slippagePips, totalSwapCost, isWalkForward, outOfSampleEfficiency,
            tradeLogsJson, notes, tenantId);
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
