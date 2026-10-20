using System;
using System.Globalization;

namespace AzureTest.Contracts.DTOs.Habits;

public record HabitLogDto(
    Guid Id,
    Guid HabitTrackerId,
    Guid UserId,
    DateOnly LocalDate,
    DateTime LoggedAt,
    string Value,
    double NumericValue,
    string? Notes,
    double? TargetSnapshot
)
{
    public DateTime LogDate => LoggedAt;
    public bool BooleanValue => Value == "true" || NumericValue > 0;

    // Backward-compatible constructor
    public HabitLogDto(
        Guid id,
        DateTime logDate,
        bool booleanValue,
        double numericValue,
        string? notes)
        : this(
            id,
            Guid.Empty,
            Guid.Empty,
            DateOnly.FromDateTime(logDate.Kind == DateTimeKind.Utc ? logDate : logDate.ToUniversalTime()),
            logDate.Kind == DateTimeKind.Utc ? logDate : logDate.ToUniversalTime(),
            booleanValue ? "true" : numericValue.ToString(CultureInfo.InvariantCulture),
            numericValue,
            notes,
            null)
    {
    }
}
