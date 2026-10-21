using AzureTest.Domain.Entities.Crypto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Crypto;

public sealed class CryptoStrategyConfiguration : IEntityTypeConfiguration<CryptoStrategy>
{
    public void Configure(EntityTypeBuilder<CryptoStrategy> b)
    {
        b.ToTable("CryptoStrategies");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name)
            .HasMaxLength(CryptoStrategy.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.Description)
            .HasMaxLength(CryptoStrategy.Rules.DescriptionMaxLength);

        b.Property(x => x.Symbol)
            .HasMaxLength(CryptoStrategy.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.StrategyType)
            .HasConversion<int>();

        b.Property(x => x.Timeframe)
            .HasConversion<int>();

        b.Property(x => x.ParametersJson)
            .HasMaxLength(CryptoStrategy.Rules.ParametersMaxLength);

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
