using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FixtureLineupConfiguration : IEntityTypeConfiguration<FixtureLineup>
{
    public void Configure(EntityTypeBuilder<FixtureLineup> builder)
    {
        builder.ToTable("FixtureLineups", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Formation).HasMaxLength(FixtureLineup.Rules.FormationMaxLength).IsRequired();

        builder.HasOne(x => x.Fixture).WithMany().HasForeignKey(x => x.FixtureId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Coach).WithMany().HasForeignKey(x => x.CoachId).OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
