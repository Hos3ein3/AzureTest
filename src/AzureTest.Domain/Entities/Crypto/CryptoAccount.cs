using System;
using AzureTest.Domain.Entities.Crypto.Enums;
using AzureTest.Domain.Entities.Crypto.Events;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Crypto;

public sealed class CryptoAccount : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int CurrencyMaxLength = 10;
        public const int ReasonMaxLength = 500;
        public const int BalancePrecision = 18;
        public const int BalanceScale = 4;
        public const int PercentPrecision = 5;
        public const int PercentScale = 2;
        public const int MaxOpenPositionsMin = 1;
        public const int MaxOpenPositionsMax = 20;
        public const int DefaultMaxOpenPositions = 5;
        public static readonly decimal InitialDefaultBalance = 10000.00m;
        public static readonly decimal MinBalance = 0.00m;
        public static readonly decimal DefaultRiskPerTrade = 1.00m;
        public static readonly decimal MinRiskPerTrade = 0.10m;
        public static readonly decimal MaxRiskPerTrade = 10.00m;
        public static readonly decimal DefaultDailyLossLimit = 3.00m;
        public static readonly decimal MinDailyLossLimit = 1.00m;
        public static readonly decimal MaxDailyLossLimit = 20.00m;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string BaseCurrency { get; private set; } = "USDT";
    public CryptoTradingMode TradingMode { get; private set; } = CryptoTradingMode.PaperTrading;
    public decimal CashBalance { get; private set; }
    public decimal Equity { get; private set; }
    public decimal MarginUsed { get; private set; }
    public decimal FreeMargin => Math.Max(Rules.MinBalance, Equity - MarginUsed);
    public decimal DailyStartingEquity { get; private set; }
    public decimal CurrentDailyLoss { get; private set; }
    public decimal DailyLossLimitPercent { get; private set; } = Rules.DefaultDailyLossLimit;
    public decimal RiskPercentagePerTrade { get; private set; } = Rules.DefaultRiskPerTrade;
    public int MaxOpenPositions { get; private set; } = Rules.DefaultMaxOpenPositions;
    public bool IsKillSwitchActive { get; private set; }
    public string? KillSwitchReason { get; private set; }
    public DateTime? KillSwitchTriggeredAtUtc { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private CryptoAccount() { }

    private CryptoAccount(
        Guid userId,
        Guid tenantId,
        string name,
        string baseCurrency,
        CryptoTradingMode tradingMode,
        decimal initialBalance,
        decimal riskPercentagePerTrade,
        decimal dailyLossLimitPercent,
        int maxOpenPositions)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Name = name;
        BaseCurrency = baseCurrency;
        TradingMode = tradingMode;
        CashBalance = initialBalance;
        Equity = initialBalance;
        DailyStartingEquity = initialBalance;
        RiskPercentagePerTrade = riskPercentagePerTrade;
        DailyLossLimitPercent = dailyLossLimitPercent;
        MaxOpenPositions = maxOpenPositions;
        IsKillSwitchActive = false;
    }

    public static CryptoAccount Create(
        Guid userId,
        Guid tenantId,
        string name,
        string baseCurrency = "USDT",
        CryptoTradingMode tradingMode = CryptoTradingMode.PaperTrading,
        decimal? initialBalance = null,
        decimal? riskPercentagePerTrade = null,
        decimal? dailyLossLimitPercent = null,
        int? maxOpenPositions = null)
    {
        var balance = initialBalance ?? Rules.InitialDefaultBalance;
        var risk = riskPercentagePerTrade ?? Rules.DefaultRiskPerTrade;
        var lossLimit = dailyLossLimitPercent ?? Rules.DefaultDailyLossLimit;
        var maxPos = maxOpenPositions ?? Rules.DefaultMaxOpenPositions;

        return new CryptoAccount(
            userId,
            tenantId,
            name.Trim(),
            string.IsNullOrWhiteSpace(baseCurrency) ? "USDT" : baseCurrency.Trim().ToUpperInvariant(),
            tradingMode,
            balance,
            risk,
            lossLimit,
            maxPos);
    }

    public void UpdateBalance(decimal cashDelta, decimal marginUsed, decimal unrealizedPnl)
    {
        CashBalance = Math.Max(Rules.MinBalance, CashBalance + cashDelta);
        MarginUsed = Math.Max(Rules.MinBalance, marginUsed);
        Equity = Math.Max(Rules.MinBalance, CashBalance + unrealizedPnl);

        if (DailyStartingEquity > 0)
        {
            var loss = DailyStartingEquity - Equity;
            CurrentDailyLoss = Math.Max(Rules.MinBalance, loss);

            var lossPercent = (CurrentDailyLoss / DailyStartingEquity) * 100m;
            if (lossPercent >= DailyLossLimitPercent && !IsKillSwitchActive)
            {
                TriggerKillSwitch($"Daily loss limit breached: {lossPercent:F2}% >= {DailyLossLimitPercent:F2}%");
            }
        }
    }

    public void TriggerKillSwitch(string reason)
    {
        IsKillSwitchActive = true;
        KillSwitchReason = reason;
        KillSwitchTriggeredAtUtc = DateTime.UtcNow;
        AddDomainEvent(new CryptoKillSwitchActivatedEvent(Id, UserId, reason));
    }

    public void ResetKillSwitch()
    {
        IsKillSwitchActive = false;
        KillSwitchReason = null;
        KillSwitchTriggeredAtUtc = null;
    }

    public void ResetDailyTracking()
    {
        DailyStartingEquity = Equity;
        CurrentDailyLoss = Rules.MinBalance;
    }

    public void UpdateRiskSettings(decimal riskPercentagePerTrade, decimal dailyLossLimitPercent, int maxOpenPositions)
    {
        RiskPercentagePerTrade = Math.Clamp(riskPercentagePerTrade, Rules.MinRiskPerTrade, Rules.MaxRiskPerTrade);
        DailyLossLimitPercent = Math.Clamp(dailyLossLimitPercent, Rules.MinDailyLossLimit, Rules.MaxDailyLossLimit);
        MaxOpenPositions = Math.Clamp(maxOpenPositions, Rules.MaxOpenPositionsMin, Rules.MaxOpenPositionsMax);
    }

    public void SetTradingMode(CryptoTradingMode mode)
    {
        TradingMode = mode;
    }

    public void ResetAccount(decimal resetBalance)
    {
        CashBalance = resetBalance;
        Equity = resetBalance;
        MarginUsed = Rules.MinBalance;
        DailyStartingEquity = resetBalance;
        CurrentDailyLoss = Rules.MinBalance;
        IsKillSwitchActive = false;
        KillSwitchReason = null;
        KillSwitchTriggeredAtUtc = null;
    }
}
