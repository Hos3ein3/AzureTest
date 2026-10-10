using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLineupConfiguration : IEntityTypeConfiguration<FantasyLineup>
{
    public void Configure(EntityTypeBuilder<FantasyLineup> builder)
    {
        builder.ToTable("FantasyLineups", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Formation)
            .HasMaxLength(FantasyLineup.Rules.FormationMaxLength)
            .IsRequired();

        builder.HasOne(x => x.FantasyTeam)
            .WithMany(x => x.Lineups)
            .HasForeignKey(x => x.FantasyTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FantasyPeriod)
            .WithMany(x => x.Lineups)
            .HasForeignKey(x => x.FantasyPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CaptainPlayer)
            .WithMany()
            .HasForeignKey(x => x.CaptainPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ViceCaptainPlayer)
            .WithMany()
            .HasForeignKey(x => x.ViceCaptainPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FantasyTeamId, x.FantasyPeriodId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
