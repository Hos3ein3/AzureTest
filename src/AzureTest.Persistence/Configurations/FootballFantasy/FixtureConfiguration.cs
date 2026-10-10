using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FixtureConfiguration : IEntityTypeConfiguration<Fixture>
{
    public void Configure(EntityTypeBuilder<Fixture> builder)
    {
        builder.ToTable("Fixtures", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Round)
            .HasMaxLength(Fixture.Rules.RoundMaxLength);

        builder.Property(x => x.Venue)
            .HasMaxLength(Fixture.Rules.VenueMaxLength);

        builder.Property(x => x.Referee)
            .HasMaxLength(Fixture.Rules.RefereeMaxLength);

        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.Season)
            .WithMany(x => x.Fixtures)
            .HasForeignKey(x => x.SeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Stage)
            .WithMany(x => x.Fixtures)
            .HasForeignKey(x => x.StageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.HomeTeam)
            .WithMany()
            .HasForeignKey(x => x.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AwayTeam)
            .WithMany()
            .HasForeignKey(x => x.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
