using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class ScoringRuleConfiguration : IEntityTypeConfiguration<ScoringRule>
{
    public void Configure(EntityTypeBuilder<ScoringRule> builder)
    {
        builder.ToTable("ScoringRules", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Metric).HasConversion<int>();
        builder.Property(x => x.Position).HasConversion<int>();

        builder.HasOne(x => x.RuleSet)
            .WithMany(x => x.RulesList)
            .HasForeignKey(x => x.RuleSetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.RuleSetId, x.Metric, x.Position }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
