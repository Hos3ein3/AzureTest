using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerInjuryConfiguration : IEntityTypeConfiguration<PlayerInjury>
{
    public void Configure(EntityTypeBuilder<PlayerInjury> builder)
    {
        builder.ToTable("PlayerInjuries", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(PlayerInjury.Rules.TypeMaxLength).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(PlayerInjury.Rules.ReasonMaxLength);
        builder.Property(x => x.Severity).HasConversion<int>();

        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Fixture).WithMany().HasForeignKey(x => x.FixtureId).OnDelete(DeleteBehavior.SetNull);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
