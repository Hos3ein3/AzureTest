using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FixtureLiveUpdateConfiguration : IEntityTypeConfiguration<FixtureLiveUpdate>
{
    public void Configure(EntityTypeBuilder<FixtureLiveUpdate> builder)
    {
        builder.ToTable("FixtureLiveUpdates", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(FixtureLiveUpdate.Rules.StatusMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(FixtureLiveUpdate.Rules.DescriptionMaxLength);

        builder.HasOne(x => x.Fixture).WithMany().HasForeignKey(x => x.FixtureId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
