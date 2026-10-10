using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class TeamSeasonConfiguration : IEntityTypeConfiguration<TeamSeason>
{
    public void Configure(EntityTypeBuilder<TeamSeason> builder)
    {
        builder.ToTable("TeamSeasons", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupName)
            .HasMaxLength(TeamSeason.Rules.GroupNameMaxLength);

        builder.HasOne(x => x.Team)
            .WithMany(x => x.TeamSeasons)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Season)
            .WithMany(x => x.TeamSeasons)
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TeamId, x.SeasonId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
