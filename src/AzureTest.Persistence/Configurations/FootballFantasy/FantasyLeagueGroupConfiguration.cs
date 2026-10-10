using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLeagueGroupConfiguration : IEntityTypeConfiguration<FantasyLeagueGroup>
{
    public void Configure(EntityTypeBuilder<FantasyLeagueGroup> builder)
    {
        builder.ToTable("FantasyLeagueGroups", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(FantasyLeagueGroup.Rules.NameMaxLength)
            .IsRequired();

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.Groups)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
