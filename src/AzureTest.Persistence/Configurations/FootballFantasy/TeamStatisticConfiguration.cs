using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class TeamStatisticConfiguration : IEntityTypeConfiguration<TeamStatistic>
{
    public void Configure(EntityTypeBuilder<TeamStatistic> builder)
    {
        builder.ToTable("TeamStatistics", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Form).HasMaxLength(TeamStatistic.Rules.FormMaxLength);

        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Season).WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
