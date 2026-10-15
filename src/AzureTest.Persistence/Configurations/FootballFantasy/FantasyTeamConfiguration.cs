using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyTeamConfiguration : IEntityTypeConfiguration<FantasyTeam>
{
    public void Configure(EntityTypeBuilder<FantasyTeam> builder)
    {
        builder.ToTable("FantasyTeams", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(FantasyTeam.Rules.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.LogoUrl)
            .HasColumnType("text");

        builder.Property(x => x.CreditsLeft)
            .HasPrecision(FantasyTeam.Rules.CreditsPrecision, FantasyTeam.Rules.CreditsScale);

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.FantasyTeams)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Coach)
            .WithMany()
            .HasForeignKey(x => x.CoachId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.FantasyLeagueId, x.UserId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
