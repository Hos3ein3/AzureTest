using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerTransferConfiguration : IEntityTypeConfiguration<PlayerTransfer>
{
    public void Configure(EntityTypeBuilder<PlayerTransfer> builder)
    {
        builder.ToTable("PlayerTransfers", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Fee).HasMaxLength(PlayerTransfer.Rules.FeeMaxLength);
        builder.Property(x => x.Type).HasConversion<int>();

        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FromTeam).WithMany().HasForeignKey(x => x.FromTeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ToTeam).WithMany().HasForeignKey(x => x.ToTeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
