using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Pomodoro;

namespace AzureTest.MVC.Models;

public sealed class PomodoroIndexViewModel
{
    public PomodoroSettingDto Setting { get; set; } = default!;
    public int TodayFocusMinutes { get; set; }
    public int TodayCompletedCount { get; set; }
    public int CurrentStreak { get; set; }
}

public sealed class PomodoroAnalyticsViewModel
{
    public PomodoroAnalyticsDto Analytics { get; set; } = default!;
    public IReadOnlyList<PomodoroSessionDto> RecentSessions { get; set; } = [];
    public PomodoroFilter Filter { get; set; } = new();
}
