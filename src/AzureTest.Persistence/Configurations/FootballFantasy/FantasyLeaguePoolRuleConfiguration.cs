using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLeaguePoolRuleConfiguration : IEntityTypeConfiguration<FantasyLeaguePoolRule>
{
    public void Configure(EntityTypeBuilder<FantasyLeaguePoolRule> builder)
    {
        builder.ToTable("FantasyLeaguePoolRules", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScopeValue)
            .HasMaxLength(FantasyLeaguePoolRule.Rules.ScopeValueMaxLength)
            .IsRequired();

        builder.Property(x => x.ScopeType).HasConversion<int>();

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.PoolRules)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
