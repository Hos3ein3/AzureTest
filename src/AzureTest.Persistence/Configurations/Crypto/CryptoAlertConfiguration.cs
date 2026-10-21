using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoAlertConfiguration : IEntityTypeConfiguration<CryptoAlert>
{
    public void Configure(EntityTypeBuilder<CryptoAlert> b)
    {
        b.ToTable("CryptoAlerts");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(CryptoAlert.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.AlertType)
            .HasConversion<int>();

        b.Property(x => x.TargetValue)
            .HasPrecision(CryptoAlert.Rules.ValuePrecision, CryptoAlert.Rules.ValueScale);

        b.Property(x => x.Message)
            .HasMaxLength(CryptoAlert.Rules.MessageMaxLength)
            .IsRequired();

        b.Property(x => x.Channel)
            .HasMaxLength(CryptoAlert.Rules.ChannelMaxLength)
            .IsRequired();

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => x.IsActive);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
