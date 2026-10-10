using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyPeriodFixtureConfiguration : IEntityTypeConfiguration<FantasyPeriodFixture>
{
    public void Configure(EntityTypeBuilder<FantasyPeriodFixture> builder)
    {
        builder.ToTable("FantasyPeriodFixtures", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.FantasyPeriod)
            .WithMany(x => x.PeriodFixtures)
            .HasForeignKey(x => x.FantasyPeriodId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Fixture)
            .WithMany()
            .HasForeignKey(x => x.FixtureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FantasyPeriodId, x.FixtureId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
