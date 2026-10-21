using AzureTest.Domain.Entities.Forex;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Forex;

public sealed class ForexApiSettingConfiguration : IEntityTypeConfiguration<ForexApiSetting>
{
    public void Configure(EntityTypeBuilder<ForexApiSetting> b)
    {
        b.ToTable("ForexApiSettings");
        b.HasKey(x => x.Id);

        b.Property(x => x.TwelveDataApiKey)
            .HasMaxLength(ForexApiSetting.Rules.KeyMaxLength);

        b.Property(x => x.AlphaVantageApiKey)
            .HasMaxLength(ForexApiSetting.Rules.KeyMaxLength);

        b.Property(x => x.FredApiKey)
            .HasMaxLength(ForexApiSetting.Rules.KeyMaxLength);

        b.Property(x => x.FinnhubApiKey)
            .HasMaxLength(ForexApiSetting.Rules.KeyMaxLength);

        b.Property(x => x.OandaApiKey)
            .HasMaxLength(ForexApiSetting.Rules.KeyMaxLength);

        b.Property(x => x.OandaAccountId)
            .HasMaxLength(ForexApiSetting.Rules.KeyMaxLength);

        b.Property(x => x.PrimaryPriceProvider)
            .HasMaxLength(ForexApiSetting.Rules.ProviderMaxLength)
            .IsRequired();

        b.HasIndex(x => x.UserId).IsUnique();
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
