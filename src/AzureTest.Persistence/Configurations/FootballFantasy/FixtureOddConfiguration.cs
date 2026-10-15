using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FixtureOddConfiguration : IEntityTypeConfiguration<FixtureOdd>
{
    public void Configure(EntityTypeBuilder<FixtureOdd> builder)
    {
        builder.ToTable("FixtureOdds", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ValueLabel).HasMaxLength(FixtureOdd.Rules.ValueLabelMaxLength).IsRequired();
        builder.Property(x => x.Price).HasPrecision(10, 2);
        builder.Property(x => x.MarketType).HasConversion<int>();

        builder.HasOne(x => x.Fixture).WithMany().HasForeignKey(x => x.FixtureId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Bookmaker).WithMany(x => x.Odds).HasForeignKey(x => x.BookmakerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
