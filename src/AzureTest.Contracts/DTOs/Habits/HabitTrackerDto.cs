

using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums;

namespace AzureTest.Contracts.DTOs.Habits;

public record HabitTrackerDto(
    Guid Id,
    string Name,
    string? Description,
    HabitType Type,
    HabitFrequency Frequency,
    double Goal,
    Guid UserId,
    List<HabitLogDto> Logs
);
