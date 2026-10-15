using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class TeamTrophyConfiguration : IEntityTypeConfiguration<TeamTrophy>
{
    public void Configure(EntityTypeBuilder<TeamTrophy> builder)
    {
        builder.ToTable("TeamTrophies", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeagueName).HasMaxLength(TeamTrophy.Rules.LeagueNameMaxLength).IsRequired();
        builder.Property(x => x.Season).HasMaxLength(TeamTrophy.Rules.SeasonMaxLength).IsRequired();
        builder.Property(x => x.Place).HasMaxLength(TeamTrophy.Rules.PlaceMaxLength).IsRequired();

        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Coach).WithMany().HasForeignKey(x => x.CoachId).OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
