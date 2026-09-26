using System;

namespace AzureTest.Contracts.DTOs.Habits;

public record HabitLogDto(
    Guid Id,
    DateTime LogDate,
    bool BooleanValue,
    double NumericValue,
    string? Notes
);
