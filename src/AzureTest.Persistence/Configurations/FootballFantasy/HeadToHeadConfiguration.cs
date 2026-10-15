using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class HeadToHeadConfiguration : IEntityTypeConfiguration<HeadToHead>
{
    public void Configure(EntityTypeBuilder<HeadToHead> builder)
    {
        builder.ToTable("HeadToHeads", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Team1).WithMany().HasForeignKey(x => x.Team1Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Team2).WithMany().HasForeignKey(x => x.Team2Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
