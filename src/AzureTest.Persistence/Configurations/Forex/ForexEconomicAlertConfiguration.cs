using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexEconomicAlertConfiguration : IEntityTypeConfiguration<ForexEconomicAlert>
{
    public void Configure(EntityTypeBuilder<ForexEconomicAlert> b)
    {
        b.ToTable("ForexEconomicAlerts");
        b.HasKey(x => x.Id);

        b.Property(x => x.EventName)
            .HasMaxLength(ForexEconomicAlert.Rules.EventNameMaxLength)
            .IsRequired();

        b.Property(x => x.Currency)
            .HasMaxLength(ForexEconomicAlert.Rules.CurrencyMaxLength)
            .IsRequired();

        b.Property(x => x.ImpactLevel)
            .HasConversion<int>();

        b.Property(x => x.Notes)
            .HasMaxLength(ForexEconomicAlert.Rules.NoteMaxLength);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
