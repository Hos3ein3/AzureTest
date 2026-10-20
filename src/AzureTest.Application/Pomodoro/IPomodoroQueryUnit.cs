using System;
using System.Collections.Generic;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroAnalytics;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroSessions;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroSetting;
using AzureTest.Contracts.DTOs.Pomodoro;

namespace AzureTest.Application.Pomodoro;

public interface IPomodoroQueryUnit
{
    IQueryHandler<GetPomodoroSessionsQuery, Result<IReadOnlyList<PomodoroSessionDto>>> GetSessions { get; }
    IQueryHandler<GetPomodoroAnalyticsQuery, Result<PomodoroAnalyticsDto>> GetAnalytics { get; }
    IQueryHandler<GetPomodoroSettingQuery, Result<PomodoroSettingDto>> GetSetting { get; }
}

public sealed class PomodoroQueryUnit : IPomodoroQueryUnit
{
    public IQueryHandler<GetPomodoroSessionsQuery, Result<IReadOnlyList<PomodoroSessionDto>>> GetSessions { get; }
    public IQueryHandler<GetPomodoroAnalyticsQuery, Result<PomodoroAnalyticsDto>> GetAnalytics { get; }
    public IQueryHandler<GetPomodoroSettingQuery, Result<PomodoroSettingDto>> GetSetting { get; }

    public PomodoroQueryUnit(
        IQueryHandler<GetPomodoroSessionsQuery, Result<IReadOnlyList<PomodoroSessionDto>>> getSessions,
        IQueryHandler<GetPomodoroAnalyticsQuery, Result<PomodoroAnalyticsDto>> getAnalytics,
        IQueryHandler<GetPomodoroSettingQuery, Result<PomodoroSettingDto>> getSetting)
    {
        GetSessions = getSessions;
        GetAnalytics = getAnalytics;
        GetSetting = getSetting;
    }
}
