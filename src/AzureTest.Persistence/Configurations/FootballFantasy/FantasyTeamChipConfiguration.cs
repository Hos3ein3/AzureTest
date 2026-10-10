using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyTeamChipConfiguration : IEntityTypeConfiguration<FantasyTeamChip>
{
    public void Configure(EntityTypeBuilder<FantasyTeamChip> builder)
    {
        builder.ToTable("FantasyTeamChips", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChipType).HasConversion<int>();

        builder.HasOne(x => x.FantasyTeam)
            .WithMany(x => x.Chips)
            .HasForeignKey(x => x.FantasyTeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
