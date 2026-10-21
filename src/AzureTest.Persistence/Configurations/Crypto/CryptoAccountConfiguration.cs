using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoAccountConfiguration : IEntityTypeConfiguration<CryptoAccount>
{
    public void Configure(EntityTypeBuilder<CryptoAccount> b)
    {
        b.ToTable("CryptoAccounts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(CryptoAccount.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.BaseCurrency)
            .HasMaxLength(CryptoAccount.Rules.CurrencyMaxLength)
            .IsRequired();

        b.Property(x => x.TradingMode)
            .HasConversion<int>();

        b.Property(x => x.CashBalance)
            .HasPrecision(CryptoAccount.Rules.BalancePrecision, CryptoAccount.Rules.BalanceScale);

        b.Property(x => x.Equity)
            .HasPrecision(CryptoAccount.Rules.BalancePrecision, CryptoAccount.Rules.BalanceScale);

        b.Property(x => x.MarginUsed)
            .HasPrecision(CryptoAccount.Rules.BalancePrecision, CryptoAccount.Rules.BalanceScale);

        b.Property(x => x.DailyStartingEquity)
            .HasPrecision(CryptoAccount.Rules.BalancePrecision, CryptoAccount.Rules.BalanceScale);

        b.Property(x => x.CurrentDailyLoss)
            .HasPrecision(CryptoAccount.Rules.BalancePrecision, CryptoAccount.Rules.BalanceScale);

        b.Property(x => x.DailyLossLimitPercent)
            .HasPrecision(CryptoAccount.Rules.PercentPrecision, CryptoAccount.Rules.PercentScale);

        b.Property(x => x.RiskPercentagePerTrade)
            .HasPrecision(CryptoAccount.Rules.PercentPrecision, CryptoAccount.Rules.PercentScale);

        b.Property(x => x.KillSwitchReason)
            .HasMaxLength(CryptoAccount.Rules.ReasonMaxLength);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
