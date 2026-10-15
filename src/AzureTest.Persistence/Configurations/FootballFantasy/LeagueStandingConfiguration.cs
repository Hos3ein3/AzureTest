using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class LeagueStandingConfiguration : IEntityTypeConfiguration<LeagueStanding>
{
    public void Configure(EntityTypeBuilder<LeagueStanding> builder)
    {
        builder.ToTable("LeagueStandings", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Form).HasMaxLength(LeagueStanding.Rules.FormMaxLength);
        builder.Property(x => x.Status).HasMaxLength(LeagueStanding.Rules.StatusMaxLength);
        builder.Property(x => x.Description).HasMaxLength(LeagueStanding.Rules.DescriptionMaxLength);

        builder.HasOne(x => x.Season).WithMany().HasForeignKey(x => x.SeasonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
