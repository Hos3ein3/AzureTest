using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyTeamPeriodScoreConfiguration : IEntityTypeConfiguration<FantasyTeamPeriodScore>
{
    public void Configure(EntityTypeBuilder<FantasyTeamPeriodScore> builder)
    {
        builder.ToTable("FantasyTeamPeriodScores", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.FantasyTeam)
            .WithMany(x => x.PeriodScores)
            .HasForeignKey(x => x.FantasyTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FantasyPeriod)
            .WithMany(x => x.TeamPeriodScores)
            .HasForeignKey(x => x.FantasyPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FantasyTeamId, x.FantasyPeriodId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
