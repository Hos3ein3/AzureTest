using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class TeamCoachConfiguration : IEntityTypeConfiguration<TeamCoach>
{
    public void Configure(EntityTypeBuilder<TeamCoach> builder)
    {
        builder.ToTable("TeamCoaches", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role).HasConversion<int>();

        builder.HasOne(x => x.Coach)
            .WithMany(x => x.TeamCoaches)
            .HasForeignKey(x => x.CoachId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Team)
            .WithMany(x => x.TeamCoaches)
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
