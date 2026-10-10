using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerMatchStatConfiguration : IEntityTypeConfiguration<PlayerMatchStat>
{
    public void Configure(EntityTypeBuilder<PlayerMatchStat> builder)
    {
        builder.ToTable("PlayerMatchStats", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProviderRating)
            .HasPrecision(PlayerMatchStat.Rules.RatingPrecision, PlayerMatchStat.Rules.RatingScale);

        builder.HasOne(x => x.Fixture)
            .WithMany(x => x.PlayerStats)
            .HasForeignKey(x => x.FixtureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Player)
            .WithMany(x => x.MatchStats)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FixtureId, x.PlayerId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
