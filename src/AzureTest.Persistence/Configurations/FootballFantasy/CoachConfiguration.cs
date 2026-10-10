using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class CoachConfiguration : IEntityTypeConfiguration<Coach>
{
    public void Configure(EntityTypeBuilder<Coach> builder)
    {
        builder.ToTable("Coaches", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName)
            .HasMaxLength(Coach.Rules.FirstNameMaxLength)
            .IsRequired();

        builder.Property(x => x.LastName)
            .HasMaxLength(Coach.Rules.LastNameMaxLength)
            .IsRequired();

        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(Coach.Rules.PhotoUrlMaxLength);

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Coaches)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
