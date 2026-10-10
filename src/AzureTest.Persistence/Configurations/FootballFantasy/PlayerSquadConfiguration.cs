using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerSquadConfiguration : IEntityTypeConfiguration<PlayerSquad>
{
    public void Configure(EntityTypeBuilder<PlayerSquad> builder)
    {
        builder.ToTable("PlayerSquads", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Player)
            .WithMany(x => x.PlayerSquads)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TeamSeason)
            .WithMany(x => x.PlayerSquads)
            .HasForeignKey(x => x.TeamSeasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
