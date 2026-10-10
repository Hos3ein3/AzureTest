using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLeaguePositionLimitConfiguration : IEntityTypeConfiguration<FantasyLeaguePositionLimit>
{
    public void Configure(EntityTypeBuilder<FantasyLeaguePositionLimit> builder)
    {
        builder.ToTable("FantasyLeaguePositionLimits", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Position).HasConversion<int>();

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.PositionLimits)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FantasyLeagueId, x.Position }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
