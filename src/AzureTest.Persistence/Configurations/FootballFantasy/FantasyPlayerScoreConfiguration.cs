using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyPlayerScoreConfiguration : IEntityTypeConfiguration<FantasyPlayerScore>
{
    public void Configure(EntityTypeBuilder<FantasyPlayerScore> builder)
    {
        builder.ToTable("FantasyPlayerScores", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Breakdown)
            .HasColumnType("jsonb");

        builder.HasOne(x => x.FantasyLeague)
            .WithMany()
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FantasyPeriod)
            .WithMany(x => x.PlayerScores)
            .HasForeignKey(x => x.FantasyPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Fixture)
            .WithMany()
            .HasForeignKey(x => x.FixtureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FantasyLeagueId, x.FixtureId, x.PlayerId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
