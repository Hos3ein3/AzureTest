using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("Leagues", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(League.Rules.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.ShortName)
            .HasMaxLength(League.Rules.ShortNameMaxLength);

        builder.Property(x => x.Continent)
            .HasMaxLength(League.Rules.ContinentMaxLength);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(League.Rules.LogoUrlMaxLength);

        builder.Property(x => x.Type).HasConversion<int>();
        builder.Property(x => x.Level).HasConversion<int>();

        builder.HasOne(x => x.Country)
            .WithMany(x => x.Leagues)
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
