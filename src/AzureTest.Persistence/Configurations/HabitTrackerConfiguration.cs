using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class HabitTrackerConfiguration : IEntityTypeConfiguration<HabitTracker>
{
    public void Configure(EntityTypeBuilder<HabitTracker> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        builder.Property(h => h.Description).HasMaxLength(1000);
        builder.Property(h => h.Type).IsRequired();
        builder.Property(h => h.Direction).IsRequired();
        builder.Property(h => h.Target).IsRequired(false);
        builder.Property(h => h.Schedule).HasMaxLength(100);
        builder.Property(h => h.Frequency).IsRequired();
        builder.Property(h => h.TimeZone).IsRequired().HasMaxLength(100).HasDefaultValue("UTC");
        builder.Property(h => h.IsArchived).HasDefaultValue(false);
        builder.Property(h => h.IsPrivate).HasDefaultValue(false);
        builder.Property(h => h.TypeConfig).HasColumnType("text");

        builder.Ignore(h => h.Goal);

        builder.HasIndex(h => h.UserId);
        builder.HasIndex(h => new { h.UserId, h.IsArchived });

        builder.HasMany(h => h.Logs)
               .WithOne(l => l.HabitTracker)
               .HasForeignKey(l => l.HabitTrackerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
