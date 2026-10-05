using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class HabitLogConfiguration : IEntityTypeConfiguration<HabitLog>
{
    public void Configure(EntityTypeBuilder<HabitLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => new { l.HabitTrackerId, l.LogDate }).IsUnique();
    }
}
