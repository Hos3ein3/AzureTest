using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerPriceConfiguration : IEntityTypeConfiguration<PlayerPrice>
{
    public void Configure(EntityTypeBuilder<PlayerPrice> builder)
    {
        builder.ToTable("PlayerPrices", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .HasPrecision(PlayerPrice.Rules.PricePrecision, PlayerPrice.Rules.PriceScale);

        builder.Property(x => x.Reason).HasConversion<int>();

        builder.HasOne(x => x.Player)
            .WithMany(x => x.Prices)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Season)
            .WithMany()
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
