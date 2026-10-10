using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyPeriodConfiguration : IEntityTypeConfiguration<FantasyPeriod>
{
    public void Configure(EntityTypeBuilder<FantasyPeriod> builder)
    {
        builder.ToTable("FantasyPeriods", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(FantasyPeriod.Rules.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.Periods)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.FantasyLeagueId, x.Number }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
