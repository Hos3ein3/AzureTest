using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class ScoringRuleSetConfiguration : IEntityTypeConfiguration<ScoringRuleSet>
{
    public void Configure(EntityTypeBuilder<ScoringRuleSet> builder)
    {
        builder.ToTable("ScoringRuleSets", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(ScoringRuleSet.Rules.NameMaxLength)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
