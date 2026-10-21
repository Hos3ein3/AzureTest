using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoApiSettingConfiguration : IEntityTypeConfiguration<CryptoApiSetting>
{
    public void Configure(EntityTypeBuilder<CryptoApiSetting> b)
    {
        b.ToTable("CryptoApiSettings");
        b.HasKey(x => x.Id);

        b.Property(x => x.Exchange)
            .HasConversion<int>();

        b.Property(x => x.ApiKeyMasked)
            .HasMaxLength(CryptoApiSetting.Rules.MaskedKeyMaxLength)
            .IsRequired();

        b.Property(x => x.EncryptedApiKey)
            .HasMaxLength(CryptoApiSetting.Rules.EncryptedKeyMaxLength)
            .IsRequired();

        b.Property(x => x.EncryptedApiSecret)
            .HasMaxLength(CryptoApiSetting.Rules.EncryptedKeyMaxLength)
            .IsRequired();

        b.Property(x => x.EncryptedPassphrase)
            .HasMaxLength(CryptoApiSetting.Rules.EncryptedKeyMaxLength);

        b.Property(x => x.ConnectionStatusMessage)
            .HasMaxLength(CryptoApiSetting.Rules.StatusMessageMaxLength);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => new { x.UserId, x.Exchange }).IsUnique();
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
