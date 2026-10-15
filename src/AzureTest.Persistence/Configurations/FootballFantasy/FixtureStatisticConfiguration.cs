using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FixtureStatisticConfiguration : IEntityTypeConfiguration<FixtureStatistic>
{
    public void Configure(EntityTypeBuilder<FixtureStatistic> builder)
    {
        builder.ToTable("FixtureStatistics", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(FixtureStatistic.Rules.TypeMaxLength).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(FixtureStatistic.Rules.ValueMaxLength).IsRequired();

        builder.HasOne(x => x.Fixture).WithMany().HasForeignKey(x => x.FixtureId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
