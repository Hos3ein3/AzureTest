using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class BookmakerConfiguration : IEntityTypeConfiguration<Bookmaker>
{
    public void Configure(EntityTypeBuilder<Bookmaker> builder)
    {
        builder.ToTable("Bookmakers", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(Bookmaker.Rules.NameMaxLength).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(Bookmaker.Rules.UrlMaxLength);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
