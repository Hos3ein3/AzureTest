using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexAccountConfiguration : IEntityTypeConfiguration<ForexAccount>
{
    public void Configure(EntityTypeBuilder<ForexAccount> b)
    {
        b.ToTable("ForexAccounts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(ForexAccount.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.BaseCurrency)
            .HasMaxLength(ForexAccount.Rules.CurrencyMaxLength)
            .IsRequired();

        b.Property(x => x.Balance)
            .HasPrecision(ForexAccount.Rules.DecimalPrecision, ForexAccount.Rules.DecimalScale);

        b.Property(x => x.Equity)
            .HasPrecision(ForexAccount.Rules.DecimalPrecision, ForexAccount.Rules.DecimalScale);

        b.Property(x => x.UsedMargin)
            .HasPrecision(ForexAccount.Rules.DecimalPrecision, ForexAccount.Rules.DecimalScale);

        b.Property(x => x.FreeMargin)
            .HasPrecision(ForexAccount.Rules.DecimalPrecision, ForexAccount.Rules.DecimalScale);

        b.Property(x => x.MarginLevelPercent)
            .HasPrecision(ForexAccount.Rules.DecimalPrecision, ForexAccount.Rules.DecimalScale);

        b.Property(x => x.PeakEquity)
            .HasPrecision(ForexAccount.Rules.DecimalPrecision, ForexAccount.Rules.DecimalScale);

        b.Property(x => x.DailyStartingBalance)
            .HasPrecision(ForexAccount.Rules.DecimalPrecision, ForexAccount.Rules.DecimalScale);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
