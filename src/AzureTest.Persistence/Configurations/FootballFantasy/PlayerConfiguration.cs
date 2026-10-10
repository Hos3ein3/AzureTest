using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("Players", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(Player.Rules.FirstNameMaxLength)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(Player.Rules.LastNameMaxLength)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(Player.Rules.DisplayNameMaxLength)
            .IsRequired();

        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(Player.Rules.PhotoUrlMaxLength);

        builder.Property(x => x.Position).HasConversion<int>();
        builder.Property(x => x.PreferredFoot).HasConversion<int>();

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Players)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
