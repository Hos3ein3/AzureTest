using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FixtureEventConfiguration : IEntityTypeConfiguration<FixtureEvent>
{
    public void Configure(EntityTypeBuilder<FixtureEvent> builder)
    {
        builder.ToTable("FixtureEvents", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Detail)
            .HasMaxLength(FixtureEvent.Rules.DetailMaxLength);

        builder.Property(x => x.Type).HasConversion<int>();

        builder.HasOne(x => x.Fixture)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.FixtureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Team)
            .WithMany()
            .HasForeignKey(x => x.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Player)
            .WithMany()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RelatedPlayer)
            .WithMany()
            .HasForeignKey(x => x.RelatedPlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
