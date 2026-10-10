using AzureTest.Domain.Entities.FootballFantasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations.FootballFantasy;

public sealed class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements", "FootballFantasy");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(Achievement.Rules.TitleMaxLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(Achievement.Rules.DescriptionMaxLength)
            .IsRequired();

        builder.Property(x => x.BadgeIcon)
            .HasMaxLength(Achievement.Rules.BadgeIconMaxLength)
            .IsRequired();

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
