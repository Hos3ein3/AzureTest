using AzureTest.Domain.Entities.Trading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.Trading;

public sealed class TradingWatchlistConfiguration : IEntityTypeConfiguration<TradingWatchlist>
{
    public void Configure(EntityTypeBuilder<TradingWatchlist> b)
    {
        b.ToTable("TradingWatchlists");
        b.HasKey(x => x.Id);

        b.Property(x => x.Symbol)
            .HasMaxLength(TradingWatchlist.Rules.SymbolMaxLength)
            .IsRequired();

        b.Property(x => x.AssetName)
            .HasMaxLength(TradingWatchlist.Rules.NameMaxLength)
            .IsRequired();

        b.Property(x => x.AssetClass)
            .HasConversion<int>();

        b.HasIndex(x => x.UserId);
        b.HasIndex(x => x.TenantId);
        b.HasIndex(x => new { x.UserId, x.Symbol }).IsUnique();
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
