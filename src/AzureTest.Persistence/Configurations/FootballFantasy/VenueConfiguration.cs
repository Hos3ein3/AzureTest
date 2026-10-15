using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
    public void Configure(EntityTypeBuilder<Venue> builder)
    {
        builder.ToTable("Venues", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(Venue.Rules.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.City)
            .HasMaxLength(Venue.Rules.CityMaxLength);

        builder.Property(x => x.Address)
            .HasMaxLength(Venue.Rules.AddressMaxLength);

        builder.Property(x => x.Surface)
            .HasMaxLength(Venue.Rules.SurfaceMaxLength);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(Venue.Rules.ImageUrlMaxLength);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Venues)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
