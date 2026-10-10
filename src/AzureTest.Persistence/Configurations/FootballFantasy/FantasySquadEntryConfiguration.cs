using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasySquadEntryConfiguration : IEntityTypeConfiguration<FantasySquadEntry>
{
    public void Configure(EntityTypeBuilder<FantasySquadEntry> builder)
    {
        builder.ToTable("FantasySquadEntries", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PricePaid)
            .HasPrecision(FantasySquadEntry.Rules.PricePrecision, FantasySquadEntry.Rules.PriceScale);

        builder.HasOne(x => x.FantasyTeam)
            .WithMany(x => x.SquadEntries)
            .HasForeignKey(x => x.FantasyTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.FantasyTeamId, x.PlayerId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
