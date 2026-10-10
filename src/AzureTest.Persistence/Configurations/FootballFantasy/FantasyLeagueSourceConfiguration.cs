using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLeagueSourceConfiguration : IEntityTypeConfiguration<FantasyLeagueSource>
{
    public void Configure(EntityTypeBuilder<FantasyLeagueSource> builder)
    {
        builder.ToTable("FantasyLeagueSources", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.Sources)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Season)
            .WithMany()
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FantasyLeagueId, x.SeasonId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
