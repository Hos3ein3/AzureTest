using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerSeasonStatisticConfiguration : IEntityTypeConfiguration<PlayerSeasonStatistic>
{
    public void Configure(EntityTypeBuilder<PlayerSeasonStatistic> builder)
    {
        builder.ToTable("PlayerSeasonStatistics", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Rating).HasPrecision(4, 2);

        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Season).WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
