using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class SeasonConfiguration : IEntityTypeConfiguration<Season>
{
    public void Configure(EntityTypeBuilder<Season> builder)
    {
        builder.ToTable("Seasons", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(Season.Rules.NameMaxLength)
            .IsRequired();

        builder.HasOne(x => x.League)
            .WithMany(x => x.Seasons)
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.LeagueId, x.Name }).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
