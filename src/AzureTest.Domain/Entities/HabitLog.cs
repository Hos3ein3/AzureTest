using System;
using System.Globalization;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class HabitLog : BaseEntity, IAudit, ISoftDelete
{
    public Guid HabitTrackerId { get; private set; }
    public Guid UserId { get; private set; }
    public DateOnly LocalDate { get; private set; }
    public DateTime LoggedAt { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public double? NumericValue { get; private set; }
    public string? Notes { get; private set; }
    public double? TargetSnapshot { get; private set; }

    public HabitTracker HabitTracker { get; private set; } = null!;

    // Backward compatibility helpers
    public DateTime LogDate => LoggedAt;
    public bool BooleanValue => Value == "true" || (NumericValue.HasValue && NumericValue.Value > 0);

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    private HabitLog() { }

    public static HabitLog Create(
        Guid habitTrackerId,
        Guid userId,
        DateOnly localDate,
        DateTime loggedAt,
        string value,
        double? numericValue,
        string? notes,
        double? targetSnapshot)
    {
        return new HabitLog
        {
            Id = Guid.NewGuid(),
            HabitTrackerId = habitTrackerId,
            UserId = userId,
            LocalDate = localDate,
            LoggedAt = loggedAt,
            Value = value,
            NumericValue = numericValue,
            Notes = notes,
            TargetSnapshot = targetSnapshot
        };
    }

    public static HabitLog Create(
        Guid habitTrackerId,
        DateTime logDate,
        bool booleanValue,
        double numericValue,
        string? notes)
    {
        var utcNow = logDate.Kind == DateTimeKind.Utc ? logDate : logDate.ToUniversalTime();
        var localDate = DateOnly.FromDateTime(utcNow);
        var valStr = booleanValue ? "true" : numericValue.ToString(CultureInfo.InvariantCulture);

        return new HabitLog
        {
            Id = Guid.NewGuid(),
            HabitTrackerId = habitTrackerId,
            UserId = Guid.Empty,
            LocalDate = localDate,
            LoggedAt = utcNow,
            Value = valStr,
            NumericValue = numericValue,
            Notes = notes,
            TargetSnapshot = null
        };
    }

    public void Update(string value, double? numericValue, string? notes, DateTime loggedAt)
    {
        Value = value;
        NumericValue = numericValue;
        Notes = notes;
        LoggedAt = loggedAt;
    }

    public void Update(bool booleanValue, double numericValue, string? notes)
    {
        Value = booleanValue ? "true" : numericValue.ToString(CultureInfo.InvariantCulture);
        NumericValue = numericValue;
        Notes = notes;
        LoggedAt = DateTime.UtcNow;
    }

    public void SetTargetSnapshot(double targetSnapshot)
    {
        TargetSnapshot = targetSnapshot;
    }
}
