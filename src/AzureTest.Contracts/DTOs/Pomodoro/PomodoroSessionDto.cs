using System;
using AzureTest.Domain.Enums.Pomodoro;

namespace AzureTest.Contracts.DTOs.Pomodoro;

public sealed record PomodoroSessionDto(
    Guid Id,
    Guid UserId,
    string Title,
    string? Category,
    Guid? TaskItemId,
    string? TaskTitle,
    PomodoroSessionType SessionType,
    PomodoroStatus Status,
    int PlannedDurationSeconds,
    int ActualDurationSeconds,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    int InterruptionCount,
    string? Notes,
    TimerSkin UsedSkin,
    DateTime CreatedOn);
