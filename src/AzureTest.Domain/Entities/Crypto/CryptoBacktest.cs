using System;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoBacktest : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int StrategyNameMaxLength = 100;
        public const int SymbolMaxLength = 20;
        public const int JsonDataMaxLength = 50000;
        public const int CapitalPrecision = 18;
        public const int CapitalScale = 4;
        public const int MetricPrecision = 8;
        public const int MetricScale = 4;
        public static readonly decimal ZeroValue = 0.00m;
    }

    public Guid? StrategyId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string StrategyName { get; private set; } = string.Empty;
    public CryptoStrategyType StrategyType { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public CryptoTimeframe Timeframe { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal InitialCapital { get; private set; }
    public decimal FinalCapital { get; private set; }
    public decimal TotalNetProfit => FinalCapital - InitialCapital;
    public decimal ReturnPercentage => InitialCapital > 0 ? ((FinalCapital - InitialCapital) / InitialCapital) * 100m : Rules.ZeroValue;
    public int TotalTrades { get; private set; }
    public int WinningTrades { get; private set; }
    public int LosingTrades { get; private set; }
    public decimal WinRate { get; private set; }
    public decimal ProfitFactor { get; private set; }
    public decimal MaxDrawdownPercent { get; private set; }
    public decimal SharpeRatio { get; private set; }
    public decimal CommissionFeePercent { get; private set; }
    public decimal SlippagePercent { get; private set; }
    public decimal TotalFeesPaid { get; private set; }
    public string EquityCurveJson { get; private set; } = "[]";
    public string WalkForwardSummaryJson { get; private set; } = "{}";

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private CryptoBacktest() { }

    private CryptoBacktest(
        Guid? strategyId,
        Guid userId,
        Guid tenantId,
        string strategyName,
        CryptoStrategyType strategyType,
        string symbol,
        CryptoTimeframe timeframe,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        decimal finalCapital,
        int totalTrades,
        int winningTrades,
        int losingTrades,
        decimal winRate,
        decimal profitFactor,
        decimal maxDrawdownPercent,
        decimal sharpeRatio,
        decimal commissionFeePercent,
        decimal slippagePercent,
        decimal totalFeesPaid,
        string equityCurveJson,
        string walkForwardSummaryJson)
    {
        Id = Guid.NewGuid();
        StrategyId = strategyId;
        UserId = userId;
        TenantId = tenantId;
        StrategyName = strategyName;
        StrategyType = strategyType;
        Symbol = symbol;
        Timeframe = timeframe;
        StartDate = startDate;
        EndDate = endDate;
        InitialCapital = initialCapital;
        FinalCapital = finalCapital;
        TotalTrades = totalTrades;
        WinningTrades = winningTrades;
        LosingTrades = losingTrades;
        WinRate = winRate;
        ProfitFactor = profitFactor;
        MaxDrawdownPercent = maxDrawdownPercent;
        SharpeRatio = sharpeRatio;
        CommissionFeePercent = commissionFeePercent;
        SlippagePercent = slippagePercent;
        TotalFeesPaid = totalFeesPaid;
        EquityCurveJson = equityCurveJson;
        WalkForwardSummaryJson = walkForwardSummaryJson;
    }

    public static CryptoBacktest Create(
        Guid? strategyId,
        Guid userId,
        Guid tenantId,
        string strategyName,
        CryptoStrategyType strategyType,
        string symbol,
        CryptoTimeframe timeframe,
        DateTime startDate,
        DateTime endDate,
        decimal initialCapital,
        decimal finalCapital,
        int totalTrades,
        int winningTrades,
        int losingTrades,
        decimal winRate,
        decimal profitFactor,
        decimal maxDrawdownPercent,
        decimal sharpeRatio,
        decimal commissionFeePercent,
        decimal slippagePercent,
        decimal totalFeesPaid,
        string? equityCurveJson = null,
        string? walkForwardSummaryJson = null)
    {
        return new CryptoBacktest(
            strategyId,
            userId,
            tenantId,
            strategyName.Trim(),
            strategyType,
            symbol.Trim().ToUpperInvariant(),
            timeframe,
            startDate,
            endDate,
            initialCapital,
            finalCapital,
            totalTrades,
            winningTrades,
            losingTrades,
            winRate,
            profitFactor,
            maxDrawdownPercent,
            sharpeRatio,
            commissionFeePercent,
            slippagePercent,
            totalFeesPaid,
            string.IsNullOrWhiteSpace(equityCurveJson) ? "[]" : equityCurveJson,
            string.IsNullOrWhiteSpace(walkForwardSummaryJson) ? "{}" : walkForwardSummaryJson);
    }
}
