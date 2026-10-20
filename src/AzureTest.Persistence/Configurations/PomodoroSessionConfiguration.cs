using AzureTest.Domain.Entities.Pomodoro;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AzureTest.Persistence.Configurations;

public sealed class PomodoroSessionConfiguration : IEntityTypeConfiguration<PomodoroSession>
{
    public void Configure(EntityTypeBuilder<PomodoroSession> builder)
    {
        builder.ToTable("PomodoroSessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(PomodoroSession.Rules.TitleMaxLength);

        builder.Property(x => x.Category)
            .HasMaxLength(PomodoroSession.Rules.CategoryMaxLength);

        builder.Property(x => x.Notes)
            .HasMaxLength(PomodoroSession.Rules.NotesMaxLength);

        builder.Property(x => x.SessionType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.UsedSkin)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.PlannedDurationSeconds)
            .IsRequired();

        builder.Property(x => x.ActualDurationSeconds)
            .IsRequired();

        builder.Property(x => x.StartedAtUtc)
            .IsRequired();

        builder.HasOne(x => x.TaskItem)
            .WithMany()
            .HasForeignKey(x => x.TaskItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TaskItemId);
        builder.HasIndex(x => new { x.UserId, x.StartedAtUtc });
        builder.HasIndex(x => new { x.UserId, x.Status });
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}
