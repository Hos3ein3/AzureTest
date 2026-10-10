using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(Country.Rules.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasMaxLength(Country.Rules.CodeMaxLength)
            .IsRequired();

        builder.Property(x => x.Continent)
            .HasMaxLength(Country.Rules.ContinentMaxLength)
            .IsRequired();

        builder.Property(x => x.FlagUrl)
            .HasMaxLength(Country.Rules.FlagUrlMaxLength);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
