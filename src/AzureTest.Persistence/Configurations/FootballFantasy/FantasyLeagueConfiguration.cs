using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLeagueConfiguration : IEntityTypeConfiguration<FantasyLeague>
{
    public void Configure(EntityTypeBuilder<FantasyLeague> builder)
    {
        builder.ToTable("FantasyLeagues", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(FantasyLeague.Rules.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.LeagueTag)
            .HasMaxLength(FantasyLeague.Rules.LeagueTagMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(FantasyLeague.Rules.DescriptionMaxLength);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(FantasyLeague.Rules.LogoUrlMaxLength);

        builder.Property(x => x.AllowedFormations)
            .HasMaxLength(FantasyLeague.Rules.AllowedFormationsMaxLength);

        builder.Property(x => x.JoinCode)
            .HasMaxLength(FantasyLeague.Rules.JoinCodeMaxLength);

        builder.Property(x => x.Budget)
            .HasPrecision(FantasyLeague.Rules.BudgetPrecision, FantasyLeague.Rules.BudgetScale);

        builder.Property(x => x.CaptainMultiplier)
            .HasPrecision(FantasyLeague.Rules.MultiplierPrecision, FantasyLeague.Rules.MultiplierScale);

        builder.Property(x => x.Visibility).HasConversion<int>();
        builder.Property(x => x.Mode).HasConversion<int>();
        builder.Property(x => x.Format).HasConversion<int>();
        builder.Property(x => x.PeriodType).HasConversion<int>();
        builder.Property(x => x.SquadMode).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.KickoutType).HasConversion<int>();

        builder.HasOne(x => x.ScoringRuleSet)
            .WithMany(x => x.FantasyLeagues)
            .HasForeignKey(x => x.ScoringRuleSetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.LeagueTag).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
