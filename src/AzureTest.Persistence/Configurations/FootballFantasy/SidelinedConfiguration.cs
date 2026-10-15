using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class SidelinedConfiguration : IEntityTypeConfiguration<Sidelined>
{
    public void Configure(EntityTypeBuilder<Sidelined> builder)
    {
        builder.ToTable("Sidelined", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(Sidelined.Rules.TypeMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(Sidelined.Rules.DescriptionMaxLength);

        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Coach).WithMany().HasForeignKey(x => x.CoachId).OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
