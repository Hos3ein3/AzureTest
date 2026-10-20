using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Pomodoro;
using AzureTest.Domain.Enums.Pomodoro;

namespace AzureTest.Application.Pomodoro.Queries.GetPomodoroAnalytics;

public sealed record GetPomodoroAnalyticsQuery(Guid UserId) : IQuery<Result<PomodoroAnalyticsDto>>;

public sealed class GetPomodoroAnalyticsQueryHandler : IQueryHandler<GetPomodoroAnalyticsQuery, Result<PomodoroAnalyticsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPomodoroAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PomodoroAnalyticsDto>> HandleAsync(GetPomodoroAnalyticsQuery query, CancellationToken cancellationToken = default)
    {
        var allSessions = await _unitOfWork.Pomodoros.GetUserSessionsAsync(query.UserId, ct: cancellationToken);

        var now = DateTime.UtcNow;
        var todayUtc = now.Date;
        var weekStartUtc = todayUtc.AddDays(-(int)now.DayOfWeek);
        var monthStartUtc = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var focusSessions = allSessions.Where(s => s.SessionType == PomodoroSessionType.Work).ToList();
        var completedFocus = focusSessions.Where(s => s.Status == PomodoroStatus.Completed).ToList();

        var totalFocusMinutesAllTime = (int)Math.Round(focusSessions.Sum(s => s.ActualDurationSeconds) / 60.0);
        var totalFocusMinutesToday = (int)Math.Round(focusSessions.Where(s => s.StartedAtUtc >= todayUtc).Sum(s => s.ActualDurationSeconds) / 60.0);
        var totalFocusMinutesThisWeek = (int)Math.Round(focusSessions.Where(s => s.StartedAtUtc >= weekStartUtc).Sum(s => s.ActualDurationSeconds) / 60.0);
        var totalFocusMinutesThisMonth = (int)Math.Round(focusSessions.Where(s => s.StartedAtUtc >= monthStartUtc).Sum(s => s.ActualDurationSeconds) / 60.0);

        var totalCompleted = allSessions.Count(s => s.Status == PomodoroStatus.Completed);
        var totalAbandoned = allSessions.Count(s => s.Status == PomodoroStatus.Abandoned);
        var completedTodayCount = completedFocus.Count(s => s.StartedAtUtc >= todayUtc);

        var totalAll = totalCompleted + totalAbandoned;
        var completionRate = totalAll > 0 ? Math.Round((double)totalCompleted / totalAll * 100.0, 1) : 100.0;

        // Daily Streak Calculation
        var activeDates = completedFocus
            .Select(s => DateOnly.FromDateTime(s.StartedAtUtc))
            .Distinct()
            .OrderByDescending(d => d)
            .ToHashSet();

        var currentStreak = 0;
        var checkDate = DateOnly.FromDateTime(now);

        // If today has activity, start checking from today, else if yesterday has activity start from yesterday
        if (!activeDates.Contains(checkDate))
        {
            checkDate = checkDate.AddDays(-1);
        }

        while (activeDates.Contains(checkDate))
        {
            currentStreak++;
            checkDate = checkDate.AddDays(-1);
        }

        // Daily Focus Trend (last 14 days)
        var dailyTrend = new List<DailyFocusPointDto>();
        for (int i = 13; i >= 0; i--)
        {
            var day = todayUtc.AddDays(-i);
            var nextDay = day.AddDays(1);
            var daySessions = focusSessions.Where(s => s.StartedAtUtc >= day && s.StartedAtUtc < nextDay).ToList();
            var mins = (int)Math.Round(daySessions.Sum(s => s.ActualDurationSeconds) / 60.0);
            dailyTrend.Add(new DailyFocusPointDto(day.ToString("MMM dd"), mins, daySessions.Count));
        }

        // Category Distribution
        var categoryGroups = focusSessions
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Category) ? "General Focus" : s.Category)
            .Select(g => new
            {
                Category = g.Key,
                Minutes = (int)Math.Round(g.Sum(s => s.ActualDurationSeconds) / 60.0)
            })
            .OrderByDescending(g => g.Minutes)
            .ToList();

        var totalCatMins = Math.Max(1, categoryGroups.Sum(c => c.Minutes));
        var categoryDistribution = categoryGroups
            .Select(c => new CategoryFocusPointDto(c.Category, c.Minutes, (int)Math.Round((double)c.Minutes / totalCatMins * 100.0)))
            .ToList();

        // Session Type Distribution
        var sessionTypeDistribution = allSessions
            .GroupBy(s => s.SessionType)
            .Select(g => new SessionTypePointDto(
                g.Key == PomodoroSessionType.Work ? "Focus" : (g.Key == PomodoroSessionType.ShortBreak ? "Short Break" : "Long Break"),
                (int)Math.Round(g.Sum(s => s.ActualDurationSeconds) / 60.0),
                g.Count()))
            .ToList();

        // Hourly Distribution (0 to 23)
        var hourlyDistribution = Enumerable.Range(0, 24)
            .Select(h =>
            {
                var hSessions = focusSessions.Where(s => s.StartedAtUtc.Hour == h);
                var mins = (int)Math.Round(hSessions.Sum(s => s.ActualDurationSeconds) / 60.0);
                return new HourlyProductivityPointDto(h, mins);
            })
            .ToList();

        var dto = new PomodoroAnalyticsDto(
            TotalFocusMinutesAllTime: totalFocusMinutesAllTime,
            TotalFocusMinutesToday: totalFocusMinutesToday,
            TotalFocusMinutesThisWeek: totalFocusMinutesThisWeek,
            TotalFocusMinutesThisMonth: totalFocusMinutesThisMonth,
            TotalCompletedSessions: totalCompleted,
            TotalAbandonedSessions: totalAbandoned,
            CompletedTodayCount: completedTodayCount,
            CurrentDailyStreak: currentStreak,
            CompletionRatePercentage: completionRate,
            DailyTrend: dailyTrend,
            CategoryDistribution: categoryDistribution,
            SessionTypeDistribution: sessionTypeDistribution,
            HourlyDistribution: hourlyDistribution);

        return Result<PomodoroAnalyticsDto>.Success(dto);
    }
}
