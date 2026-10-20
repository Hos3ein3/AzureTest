using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Habits;

public record HabitTrackerDto(
    Guid Id,
    string Name,
    string? Description,
    HabitType Type,
    HabitDirection Direction,
    double? Target,
    string? Schedule,
    HabitFrequency Frequency,
    string TimeZone,
    bool IsArchived,
    bool IsPrivate,
    string? TypeConfig,
    Guid UserId,
    List<HabitLogDto> Logs,
    int CurrentStreak = 0,
    int LongestStreak = 0,
    bool IsCompletedToday = false,
    double TodayNumericValue = 0,
    string? TodayValue = null,
    double ProgressPercentage = 0
)
{
    public double? Goal => Target;

    // Backward-compatible constructor
    public HabitTrackerDto(
        Guid id,
        string name,
        string? description,
        HabitType type,
        HabitFrequency frequency,
        double? goal,
        Guid userId,
        List<HabitLogDto> logs)
        : this(
            id,
            name,
            description,
            type,
            HabitDirection.AtLeast,
            goal,
            frequency.ToString(),
            frequency,
            "UTC",
            false,
            false,
            null,
            userId,
            logs)
    {
    }
}
