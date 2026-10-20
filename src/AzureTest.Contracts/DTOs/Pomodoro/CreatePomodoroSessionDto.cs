using System;
using AzureTest.Domain.Enums.Pomodoro;

namespace AzureTest.Contracts.DTOs.Pomodoro;

public sealed record CreatePomodoroSessionDto(
    string Title,
    string? Category,
    PomodoroSessionType SessionType,
    int PlannedDurationSeconds,
    int ActualDurationSeconds,
    DateTime StartedAtUtc,
    DateTime? EndedAtUtc,
    PomodoroStatus Status,
    int InterruptionCount = 0,
    string? Notes = null,
    TimerSkin UsedSkin = TimerSkin.Digital);

public sealed record CompletePomodoroSessionDto(
    Guid Id,
    int ActualDurationSeconds,
    string? Notes = null);
