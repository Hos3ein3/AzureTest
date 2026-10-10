using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class FantasyLeagueInviteConfiguration : IEntityTypeConfiguration<FantasyLeagueInvite>
{
    public void Configure(EntityTypeBuilder<FantasyLeagueInvite> builder)
    {
        builder.ToTable("FantasyLeagueInvites", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(FantasyLeagueInvite.Rules.EmailMaxLength);

        builder.Property(x => x.Token)
            .HasMaxLength(FantasyLeagueInvite.Rules.TokenMaxLength)
            .IsRequired();

        builder.Property(x => x.Status).HasConversion<int>();

        builder.HasOne(x => x.FantasyLeague)
            .WithMany(x => x.Invites)
            .HasForeignKey(x => x.FantasyLeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
