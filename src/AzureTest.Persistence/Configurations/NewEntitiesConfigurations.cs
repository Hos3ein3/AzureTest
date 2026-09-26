using AzureTest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Color).HasMaxLength(20);
        builder.HasIndex(e => e.UserId);
    }
}

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Priority).HasMaxLength(50);
        builder.HasIndex(t => t.UserId);
    }
}

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
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HabitLogConfiguration : IEntityTypeConfiguration<HabitLog>
{
    public void Configure(EntityTypeBuilder<HabitLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => new { l.HabitTrackerId, l.LogDate }).IsUnique();
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(255);
        builder.HasIndex(t => t.UserId);

        builder.HasMany(t => t.Messages)
               .WithOne(m => m.Ticket)
               .HasForeignKey(m => m.TicketId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TicketMessageConfiguration : IEntityTypeConfiguration<TicketMessage>
{
    public void Configure(EntityTypeBuilder<TicketMessage> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.TicketId);
    }
}
