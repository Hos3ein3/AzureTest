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
        builder.HasIndex(h => h.UserId);

        builder.HasMany(h => h.Logs)
               .WithOne(l => l.HabitTracker)
               .HasForeignKey(l => l.HabitTrackerId)
               .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
    }
}
