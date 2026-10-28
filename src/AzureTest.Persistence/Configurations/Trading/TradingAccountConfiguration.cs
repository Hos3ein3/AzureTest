using AzureTest.Domain.Entities.Trading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Trading;

public sealed class TradingAccountConfiguration : IEntityTypeConfiguration<TradingAccount>
{
    public void Configure(EntityTypeBuilder<TradingAccount> b)
    {
        b.ToTable("TradingAccounts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(TradingAccount.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.Currency)
            .HasMaxLength(TradingAccount.Rules.CurrencyMaxLength)
            .IsRequired();

        b.Property(x => x.CashBalance)
            .HasPrecision(TradingAccount.Rules.BalancePrecision, TradingAccount.Rules.BalanceScale);

        b.Property(x => x.Equity)
            .HasPrecision(TradingAccount.Rules.BalancePrecision, TradingAccount.Rules.BalanceScale);

        b.Property(x => x.MarginUsed)
            .HasPrecision(TradingAccount.Rules.BalancePrecision, TradingAccount.Rules.BalanceScale);

        b.Property(x => x.RealizedPnl)
            .HasPrecision(TradingAccount.Rules.BalancePrecision, TradingAccount.Rules.BalanceScale);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
