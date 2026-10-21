using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexStrategyConfiguration : IEntityTypeConfiguration<ForexStrategy>
{
    public void Configure(EntityTypeBuilder<ForexStrategy> b)
    {
        b.ToTable("ForexStrategies");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(ForexStrategy.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(ForexStrategy.Rules.DescriptionMaxLength);

        b.Property(x => x.StrategyType)
            .HasConversion<int>();

        b.Property(x => x.DefaultSymbol)
            .HasMaxLength(ForexStrategy.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.DefaultTimeframe)
            .HasConversion<int>();

        b.Property(x => x.RiskPercent)
            .HasPrecision(ForexStrategy.Rules.DecimalPrecision, ForexStrategy.Rules.DecimalScale);

        b.Property(x => x.StopLossPips)
            .HasPrecision(ForexStrategy.Rules.DecimalPrecision, ForexStrategy.Rules.DecimalScale);

        b.Property(x => x.TakeProfitPips)
            .HasPrecision(ForexStrategy.Rules.DecimalPrecision, ForexStrategy.Rules.DecimalScale);

        b.Property(x => x.TrailingStopPips)
            .HasPrecision(ForexStrategy.Rules.DecimalPrecision, ForexStrategy.Rules.DecimalScale);

        b.Property(x => x.BreakEvenTriggerPips)
            .HasPrecision(ForexStrategy.Rules.DecimalPrecision, ForexStrategy.Rules.DecimalScale);

        b.Property(x => x.ParametersJson)
            .HasMaxLength(ForexStrategy.Rules.JsonParametersMaxLength);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
