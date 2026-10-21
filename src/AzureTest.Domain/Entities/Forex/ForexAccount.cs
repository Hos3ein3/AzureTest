using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Forex;

public sealed class ForexAccount : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int CurrencyMinLength = 3;
        public const int CurrencyMaxLength = 10;
        public const int DecimalPrecision = 18;
        public const int DecimalScale = 2;
        public static readonly decimal InitialBalanceMin = 100m;
        public static readonly decimal InitialBalanceMax = 10_000_000m;
        public const int LeverageMin = 1;
        public const int LeverageMax = 500;
        public const int EsmaRetailLeverageMax = 30; // ESMA cap for retail traders in EU/NL
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string BaseCurrency { get; private set; } = "USD";
    public decimal Balance { get; private set; }
    public decimal Equity { get; private set; }
    public decimal UsedMargin { get; private set; }
    public decimal FreeMargin { get; private set; }
    public decimal MarginLevelPercent { get; private set; }
    public int Leverage { get; private set; }
    public bool IsDemo { get; private set; }
    public decimal PeakEquity { get; private set; }
    public decimal DailyStartingBalance { get; private set; }
    public DateTime LastDailyResetDateUtc { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private ForexAccount() { }

    private ForexAccount(Guid userId, string name, string baseCurrency, decimal balance, int leverage, bool isDemo, Guid? tenantId = null)
    {
        UserId = userId;
        TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId;
        Name = name;
        BaseCurrency = string.IsNullOrWhiteSpace(baseCurrency) ? "USD" : baseCurrency.ToUpperInvariant();
        Balance = balance;
        Equity = balance;
        UsedMargin = 0m;
        FreeMargin = balance;
        MarginLevelPercent = 0m;
        Leverage = leverage;
        IsDemo = isDemo;
        PeakEquity = balance;
        DailyStartingBalance = balance;
        LastDailyResetDateUtc = DateTime.UtcNow.Date;
    }

    public static ForexAccount Create(Guid userId, string name, string baseCurrency, decimal balance, int leverage, bool isDemo = true, Guid? tenantId = null)
    {
        return new ForexAccount(userId, name, baseCurrency, balance, leverage, isDemo, tenantId);
    }

    public void UpdateMetrics(decimal openTradesUnrealizedPnl, decimal totalUsedMargin)
    {
        UsedMargin = Math.Max(0m, totalUsedMargin);
        Equity = Balance + openTradesUnrealizedPnl;
        FreeMargin = Math.Max(0m, Equity - UsedMargin);
        MarginLevelPercent = UsedMargin > 0m ? Math.Round((Equity / UsedMargin) * 100m, 2) : 0m;

        if (Equity > PeakEquity)
        {
            PeakEquity = Equity;
        }

        var today = DateTime.UtcNow.Date;
        if (today > LastDailyResetDateUtc)
        {
            DailyStartingBalance = Balance;
            LastDailyResetDateUtc = today;
        }
    }

    public void ApplyClosedTrade(decimal netProfit, decimal releasedMargin)
    {
        Balance += netProfit;
        UsedMargin = Math.Max(0m, UsedMargin - releasedMargin);
        Equity = Balance;
        FreeMargin = Math.Max(0m, Equity - UsedMargin);
        MarginLevelPercent = UsedMargin > 0m ? Math.Round((Equity / UsedMargin) * 100m, 2) : 0m;

        if (Equity > PeakEquity)
        {
            PeakEquity = Equity;
        }
    }

    public void ReserveMargin(decimal marginRequired)
    {
        UsedMargin += marginRequired;
        FreeMargin = Math.Max(0m, Equity - UsedMargin);
        MarginLevelPercent = UsedMargin > 0m ? Math.Round((Equity / UsedMargin) * 100m, 2) : 0m;
    }

    public void ResetAccount(decimal newBalance)
    {
        Balance = newBalance;
        Equity = newBalance;
        UsedMargin = 0m;
        FreeMargin = newBalance;
        MarginLevelPercent = 0m;
        PeakEquity = newBalance;
        DailyStartingBalance = newBalance;
        LastDailyResetDateUtc = DateTime.UtcNow.Date;
    }

    public void SoftDelete(string? deletedBy = null)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
