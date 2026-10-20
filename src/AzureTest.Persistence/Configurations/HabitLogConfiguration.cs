using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class HabitLogConfiguration : IEntityTypeConfiguration<HabitLog>
{
    public void Configure(EntityTypeBuilder<HabitLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.LocalDate).IsRequired();
        builder.Property(l => l.LoggedAt).IsRequired();
        builder.Property(l => l.Value).IsRequired().HasMaxLength(4000);
        builder.Property(l => l.Notes).HasMaxLength(1000);

        builder.Ignore(l => l.LogDate);
        builder.Ignore(l => l.BooleanValue);

        // Note: For Counter habit types, multiple event clicks per day are allowed.
        // Therefore, the index on (HabitTrackerId, LocalDate) is non-unique.
        builder.HasIndex(l => new { l.HabitTrackerId, l.LocalDate });
        builder.HasIndex(l => new { l.UserId, l.LocalDate });
    }
}
