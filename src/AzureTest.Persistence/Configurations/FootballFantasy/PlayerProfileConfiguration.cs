using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerProfileConfiguration : IEntityTypeConfiguration<PlayerProfile>
{
    public void Configure(EntityTypeBuilder<PlayerProfile> builder)
    {
        builder.ToTable("PlayerProfiles", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Biography).HasMaxLength(PlayerProfile.Rules.BiographyMaxLength);
        builder.Property(x => x.Agent).HasMaxLength(PlayerProfile.Rules.AgentMaxLength);
        builder.Property(x => x.MarketValueEur).HasPrecision(18, 2);

        builder.HasOne(x => x.Player).WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
