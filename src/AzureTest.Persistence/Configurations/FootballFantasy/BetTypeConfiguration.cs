using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class BetTypeConfiguration : IEntityTypeConfiguration<BetType>
{
    public void Configure(EntityTypeBuilder<BetType> builder)
    {
        builder.ToTable("BetTypes", "FootballFantasy");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(BetType.Rules.NameMaxLength).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(BetType.Rules.CodeMaxLength).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(BetType.Rules.DescriptionMaxLength);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
