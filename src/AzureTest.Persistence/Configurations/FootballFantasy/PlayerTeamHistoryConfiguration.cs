using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerTeamHistoryConfiguration : IEntityTypeConfiguration<PlayerTeamHistory>
{
    public void Configure(EntityTypeBuilder<PlayerTeamHistory> builder)
    {
        builder.ToTable("PlayerTeamHistories", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Team).WithMany().HasForeignKey(x => x.TeamId).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
