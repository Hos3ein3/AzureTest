using System;
using AzureTest.Domain.Enums.Pomodoro;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Pomodoro;

public sealed class PomodoroSession : AggregateRoot, ISoftDelete, IAudit, ITenantEntity
{
    public static class Rules
    {
        public const int TitleMinLength = 1;
        public const int TitleMaxLength = 100;
        public const int CategoryMaxLength = 50;
        public const int NotesMaxLength = 1000;
        public const int PlannedDurationSecondsMin = 10;
        public const int PlannedDurationSecondsMax = 86400; // up to 24 hours
        public const int ActualDurationSecondsMin = 0;
        public const int ActualDurationSecondsMax = 86400;
        public const int InterruptionCountMin = 0;
        public const int InterruptionCountMax = 100;
    }

    private PomodoroSession() { }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Title { get; private set; } = string.Empty;
    public string? Category { get; private set; }
    public Guid? TaskItemId { get; private set; }
    public TaskItem? TaskItem { get; private set; }

    public PomodoroSessionType SessionType { get; private set; }
    public PomodoroStatus Status { get; private set; }
    public int PlannedDurationSeconds { get; private set; }
    public int ActualDurationSeconds { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? EndedAtUtc { get; private set; }
    public int InterruptionCount { get; private set; }
    public string? Notes { get; private set; }
    public TimerSkin UsedSkin { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    public static PomodoroSession Create(
        Guid userId,
        string title,
        string? category,
        PomodoroSessionType sessionType,
        int plannedDurationSeconds,
        int actualDurationSeconds,
        DateTime startedAtUtc,
        DateTime? endedAtUtc,
        PomodoroStatus status,
        int interruptionCount = 0,
        string? notes = null,
        TimerSkin usedSkin = TimerSkin.Digital,
        Guid? taskItemId = null,
        Guid? tenantId = null)
    {
        var sanitizedTitle = string.IsNullOrWhiteSpace(title) ? "Focus Session" : title.Trim();
        if (sanitizedTitle.Length > Rules.TitleMaxLength)
        {
            sanitizedTitle = sanitizedTitle[..Rules.TitleMaxLength];
        }

        var sanitizedCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        if (sanitizedCategory != null && sanitizedCategory.Length > Rules.CategoryMaxLength)
        {
            sanitizedCategory = sanitizedCategory[..Rules.CategoryMaxLength];
        }

        var sanitizedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (sanitizedNotes != null && sanitizedNotes.Length > Rules.NotesMaxLength)
        {
            sanitizedNotes = sanitizedNotes[..Rules.NotesMaxLength];
        }

        var plannedSec = Math.Clamp(plannedDurationSeconds, Rules.PlannedDurationSecondsMin, Rules.PlannedDurationSecondsMax);
        var actualSec = Math.Clamp(actualDurationSeconds, Rules.ActualDurationSecondsMin, Rules.ActualDurationSecondsMax);
        var interruptions = Math.Clamp(interruptionCount, Rules.InterruptionCountMin, Rules.InterruptionCountMax);

        return new PomodoroSession
        {
            UserId = userId,
            TenantId = tenantId.HasValue && tenantId.Value != Guid.Empty ? tenantId.Value : userId,
            Title = sanitizedTitle,
            Category = sanitizedCategory,
            TaskItemId = taskItemId,
            SessionType = sessionType,
            Status = status,
            PlannedDurationSeconds = plannedSec,
            ActualDurationSeconds = actualSec,
            StartedAtUtc = startedAtUtc,
            EndedAtUtc = endedAtUtc ?? DateTime.UtcNow,
            InterruptionCount = interruptions,
            Notes = sanitizedNotes,
            UsedSkin = usedSkin,
            CreatedOn = DateTime.UtcNow
        };
    }

    public void Update(
        string title,
        string? category,
        Guid? taskItemId,
        PomodoroSessionType sessionType,
        int plannedDurationSeconds,
        int actualDurationSeconds,
        DateTime startedAtUtc,
        DateTime? endedAtUtc,
        PomodoroStatus status,
        int interruptionCount,
        string? notes,
        TimerSkin usedSkin)
    {
        var sanitizedTitle = string.IsNullOrWhiteSpace(title) ? "Focus Session" : title.Trim();
        if (sanitizedTitle.Length > Rules.TitleMaxLength)
        {
            sanitizedTitle = sanitizedTitle[..Rules.TitleMaxLength];
        }

        var sanitizedCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim();
        if (sanitizedCategory != null && sanitizedCategory.Length > Rules.CategoryMaxLength)
        {
            sanitizedCategory = sanitizedCategory[..Rules.CategoryMaxLength];
        }

        var sanitizedNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (sanitizedNotes != null && sanitizedNotes.Length > Rules.NotesMaxLength)
        {
            sanitizedNotes = sanitizedNotes[..Rules.NotesMaxLength];
        }

        Title = sanitizedTitle;
        Category = sanitizedCategory;
        TaskItemId = taskItemId;
        SessionType = sessionType;
        PlannedDurationSeconds = Math.Clamp(plannedDurationSeconds, Rules.PlannedDurationSecondsMin, Rules.PlannedDurationSecondsMax);
        ActualDurationSeconds = Math.Clamp(actualDurationSeconds, Rules.ActualDurationSecondsMin, Rules.ActualDurationSecondsMax);
        StartedAtUtc = startedAtUtc;
        EndedAtUtc = endedAtUtc;
        Status = status;
        InterruptionCount = Math.Clamp(interruptionCount, Rules.InterruptionCountMin, Rules.InterruptionCountMax);
        Notes = sanitizedNotes;
        UsedSkin = usedSkin;
        ModifiedOn = DateTime.UtcNow;
    }

    public void Complete(int actualDurationSeconds, string? notes = null)
    {
        ActualDurationSeconds = Math.Clamp(actualDurationSeconds, Rules.ActualDurationSecondsMin, Rules.ActualDurationSecondsMax);
        Status = PomodoroStatus.Completed;
        EndedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = notes.Trim();
            if (Notes.Length > Rules.NotesMaxLength)
                Notes = Notes[..Rules.NotesMaxLength];
        }
        ModifiedOn = DateTime.UtcNow;
    }

    public void Abandon(int actualDurationSeconds, string? notes = null)
    {
        ActualDurationSeconds = Math.Clamp(actualDurationSeconds, Rules.ActualDurationSecondsMin, Rules.ActualDurationSecondsMax);
        Status = PomodoroStatus.Abandoned;
        EndedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = notes.Trim();
            if (Notes.Length > Rules.NotesMaxLength)
                Notes = Notes[..Rules.NotesMaxLength];
        }
        ModifiedOn = DateTime.UtcNow;
    }

    public void RecordInterruption()
    {
        if (InterruptionCount < Rules.InterruptionCountMax)
        {
            InterruptionCount++;
            ModifiedOn = DateTime.UtcNow;
        }
    }

    public void SoftDelete(string? deletedBy)
    {
        IsDeleted = true;
        DeletedOn = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
