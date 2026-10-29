using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Trading;

public sealed class TradingAccount : AggregateRoot, IAudit, ISoftDelete, ITenantEntity
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int CurrencyMaxLength = 10;
        public const int BalancePrecision = 18;
        public const int BalanceScale = 4;
        public static readonly decimal InitialDefaultBalance = 50000.00m;
        public static readonly decimal MinBalance = 0.00m;
        public static readonly decimal MaxBalance = 1000000000.00m;
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string Currency { get; private set; } = "USD";
    public decimal CashBalance { get; private set; }
    public decimal Equity { get; private set; }
    public decimal MarginUsed { get; private set; }
    public decimal FreeMargin => Math.Max(Rules.MinBalance, Equity - MarginUsed);
    public decimal RealizedPnl { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private TradingAccount() { }

    private TradingAccount(Guid userId, Guid tenantId, string name, string currency, decimal initialBalance)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        TenantId = tenantId;
        Name = name;
        Currency = currency;
        CashBalance = initialBalance;
        Equity = initialBalance;
        MarginUsed = Rules.MinBalance;
        RealizedPnl = Rules.MinBalance;
    }

    public static TradingAccount Create(
        Guid userId,
        Guid tenantId,
        string name,
        string currency = "USD",
        decimal? initialBalance = null)
    {
        var balance = initialBalance ?? Rules.InitialDefaultBalance;
        balance = Math.Clamp(balance, Rules.MinBalance, Rules.MaxBalance);

        return new TradingAccount(
            userId,
            tenantId,
            string.IsNullOrWhiteSpace(name) ? "Paper Trading Account" : name.Trim(),
            string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant(),
            balance);
    }

    public void ResetBalance(decimal newBalance)
    {
        var clamped = Math.Clamp(newBalance, Rules.MinBalance, Rules.MaxBalance);
        CashBalance = clamped;
        Equity = clamped;
        MarginUsed = Rules.MinBalance;
        RealizedPnl = Rules.MinBalance;
    }

    public void AdjustMarginAndEquity(decimal marginDelta, decimal unrealizedPnlDelta)
    {
        MarginUsed = Math.Max(Rules.MinBalance, MarginUsed + marginDelta);
        Equity = Math.Max(Rules.MinBalance, CashBalance + unrealizedPnlDelta);
    }

    public void Recalculate(decimal activeMarginUsed, decimal totalUnrealizedPnl)
    {
        MarginUsed = Math.Max(Rules.MinBalance, activeMarginUsed);
        Equity = Math.Max(Rules.MinBalance, CashBalance + totalUnrealizedPnl);
    }

    public void ApplyRealizedPnl(decimal profitOrLoss, decimal releasedMargin)
    {
        CashBalance = Math.Max(Rules.MinBalance, CashBalance + profitOrLoss);
        MarginUsed = Math.Max(Rules.MinBalance, MarginUsed - releasedMargin);
        RealizedPnl += profitOrLoss;
        Equity = Math.Max(Rules.MinBalance, CashBalance);
    }

    public void DeductMargin(decimal margin)
    {
        MarginUsed = Math.Max(Rules.MinBalance, MarginUsed + margin);
    }

    public void ReleaseMargin(decimal margin)
    {
        MarginUsed = Math.Max(Rules.MinBalance, MarginUsed - margin);
    }
}
