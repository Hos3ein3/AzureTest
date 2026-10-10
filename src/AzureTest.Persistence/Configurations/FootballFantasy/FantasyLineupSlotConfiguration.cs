using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLineupSlotConfiguration : IEntityTypeConfiguration<FantasyLineupSlot>
{
    public void Configure(EntityTypeBuilder<FantasyLineupSlot> builder)
    {
        builder.ToTable("FantasyLineupSlots", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.FantasyLineup)
            .WithMany(x => x.Slots)
            .HasForeignKey(x => x.FantasyLineupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FantasyLineupId, x.PlayerId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
