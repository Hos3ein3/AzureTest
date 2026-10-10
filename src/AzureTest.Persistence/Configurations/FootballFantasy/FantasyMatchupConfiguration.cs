using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyMatchupConfiguration : IEntityTypeConfiguration<FantasyMatchup>
{
    public void Configure(EntityTypeBuilder<FantasyMatchup> builder)
    {
        builder.ToTable("FantasyMatchups", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StageName)
            .HasMaxLength(FantasyMatchup.Rules.StageNameMaxLength)
            .IsRequired();

        builder.Property(x => x.HomeScore)
            .HasPrecision(6, 1);

        builder.Property(x => x.AwayScore)
            .HasPrecision(6, 1);

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.Matchups)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FantasyLeagueGroup)
            .WithMany(x => x.Matchups)
            .HasForeignKey(x => x.FantasyLeagueGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Period)
            .WithMany()
            .HasForeignKey(x => x.PeriodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.HomeTeam)
            .WithMany()
            .HasForeignKey(x => x.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AwayTeam)
            .WithMany()
            .HasForeignKey(x => x.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WinnerTeam)
            .WithMany()
            .HasForeignKey(x => x.WinnerTeamId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
