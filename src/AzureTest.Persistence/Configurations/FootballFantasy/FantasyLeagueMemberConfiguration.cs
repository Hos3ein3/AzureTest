using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLeagueMemberConfiguration : IEntityTypeConfiguration<FantasyLeagueMember>
{
    public void Configure(EntityTypeBuilder<FantasyLeagueMember> builder)
    {
        builder.ToTable("FantasyLeagueMembers", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<int>();
        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FantasyLeagueId, x.UserId }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
