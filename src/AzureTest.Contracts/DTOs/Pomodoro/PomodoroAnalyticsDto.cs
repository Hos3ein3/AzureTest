using System;
using System.Collections.Generic;
using AzureTest.Domain.Enums.Pomodoro;

namespace AzureTest.Contracts.DTOs.Pomodoro;

public sealed class PomodoroFilter
{
    public PomodoroSessionType? SessionType { get; set; }
    public PomodoroStatus? Status { get; set; }
    public string? Category { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed record PomodoroAnalyticsDto(
    int TotalFocusMinutesAllTime,
    int TotalFocusMinutesToday,
    int TotalFocusMinutesThisWeek,
    int TotalFocusMinutesThisMonth,
    int TotalCompletedSessions,
    int TotalAbandonedSessions,
    int CompletedTodayCount,
    int CurrentDailyStreak,
    double CompletionRatePercentage,
    IReadOnlyList<DailyFocusPointDto> DailyTrend,
    IReadOnlyList<CategoryFocusPointDto> CategoryDistribution,
    IReadOnlyList<SessionTypePointDto> SessionTypeDistribution,
    IReadOnlyList<HourlyProductivityPointDto> HourlyDistribution);

public sealed record DailyFocusPointDto(string DateLabel, int FocusMinutes, int SessionCount);
public sealed record CategoryFocusPointDto(string Category, int FocusMinutes, int Percentage);
public sealed record SessionTypePointDto(string TypeLabel, int TotalMinutes, int SessionCount);
public sealed record HourlyProductivityPointDto(int Hour, int FocusMinutes);
