using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;

namespace AzureTest.Application.Habits;

public class HabitDaySummary
{
    public DateOnly Date { get; set; }
    public bool IsCompleted { get; set; }
    public double? NumericValue { get; set; }
    public string? Value { get; set; }
    public double Target { get; set; }
}

public class HabitComputedStats
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public bool IsCompletedToday { get; set; }
    public double TodayNumericValue { get; set; }
    public string? TodayValue { get; set; }
    public double ProgressPercentage { get; set; }
}

public static class HabitStatsCalculator
{
    public static HabitComputedStats Calculate(HabitTracker tracker, IEnumerable<HabitLog> logs, DateOnly? todayOverride = null)
    {
        var logsList = logs?.ToList() ?? new List<HabitLog>();
        var timeZoneInfo = GetTimeZone(tracker.TimeZone);
        var today = todayOverride ?? GetLocalDate(DateTime.UtcNow, timeZoneInfo);

        // Group logs by LocalDate
        var logsByDate = logsList.GroupBy(l => l.LocalDate)
                                 .ToDictionary(g => g.Key, g => g.ToList());

        // Quit / Abstinence habit has special streak semantics
        if (tracker.Type == HabitType.Quit)
        {
            return CalculateQuitStats(tracker, logsList, today);
        }

        // Evaluate completion for today
        var todaySummary = EvaluateDay(tracker, today, logsByDate.TryGetValue(today, out var tLogs) ? tLogs : null);

        // Compute current streak
        int currentStreak = 0;
        var checkDate = today;

        if (todaySummary.IsCompleted)
        {
            currentStreak = 1;
            checkDate = today.AddDays(-1);
            while (logsByDate.TryGetValue(checkDate, out var dayLogs))
            {
                var summary = EvaluateDay(tracker, checkDate, dayLogs);
                if (!summary.IsCompleted) break;
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
        }
        else
        {
            // If today is not yet completed, check if yesterday was completed
            var yesterday = today.AddDays(-1);
            if (logsByDate.TryGetValue(yesterday, out var yLogs))
            {
                var ySummary = EvaluateDay(tracker, yesterday, yLogs);
                if (ySummary.IsCompleted)
                {
                    currentStreak = 1;
                    checkDate = yesterday.AddDays(-1);
                    while (logsByDate.TryGetValue(checkDate, out var dayLogs))
                    {
                        var summary = EvaluateDay(tracker, checkDate, dayLogs);
                        if (!summary.IsCompleted) break;
                        currentStreak++;
                        checkDate = checkDate.AddDays(-1);
                    }
                }
            }
        }

        // Compute longest streak
        int longestStreak = currentStreak;
        if (logsByDate.Any())
        {
            var minDate = logsByDate.Keys.Min();
            int run = 0;
            for (var d = minDate; d <= today; d = d.AddDays(1))
            {
                var dLogs = logsByDate.TryGetValue(d, out var l) ? l : null;
                var s = EvaluateDay(tracker, d, dLogs);
                if (s.IsCompleted)
                {
                    run++;
                    if (run > longestStreak) longestStreak = run;
                }
                else
                {
                    run = 0;
                }
            }
        }

        double progress = 0;
        if (tracker.Target > 0)
        {
            progress = Math.Clamp(Math.Round((todaySummary.NumericValue ?? 0) / tracker.Target * 100, 1), 0, 100);
        }
        else if (todaySummary.IsCompleted)
        {
            progress = 100;
        }

        return new HabitComputedStats
        {
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            IsCompletedToday = todaySummary.IsCompleted,
            TodayNumericValue = todaySummary.NumericValue ?? 0,
            TodayValue = todaySummary.Value,
            ProgressPercentage = progress
        };
    }

    private static HabitComputedStats CalculateQuitStats(HabitTracker tracker, List<HabitLog> logs, DateOnly today)
    {
        // For Quit habits, any log is considered a relapse/reset event.
        var createdDate = DateOnly.FromDateTime(tracker.CreatedOn > DateTime.MinValue ? tracker.CreatedOn : DateTime.UtcNow);
        var relapseLogs = logs.Where(l => l.Value == "relapse" || l.BooleanValue || (l.NumericValue.HasValue && l.NumericValue.Value > 0)).ToList();

        DateOnly lastRelapseDate = createdDate;
        bool relapsedToday = false;

        if (relapseLogs.Any())
        {
            var maxRelapse = relapseLogs.Max(l => l.LocalDate);
            if (maxRelapse > lastRelapseDate) lastRelapseDate = maxRelapse;
            relapsedToday = relapseLogs.Any(l => l.LocalDate == today);
        }

        int daysClean = Math.Max(0, today.DayNumber - lastRelapseDate.DayNumber);
        if (relapsedToday) daysClean = 0;

        return new HabitComputedStats
        {
            CurrentStreak = daysClean,
            LongestStreak = Math.Max(daysClean, Math.Max(0, today.DayNumber - createdDate.DayNumber)),
            IsCompletedToday = !relapsedToday,
            TodayNumericValue = daysClean,
            TodayValue = relapsedToday ? "Relapse" : $"{daysClean} days clean",
            ProgressPercentage = !relapsedToday ? 100 : 0
        };
    }

    public static HabitDaySummary EvaluateDay(HabitTracker tracker, DateOnly date, List<HabitLog>? dayLogs)
    {
        if (dayLogs == null || !dayLogs.Any())
        {
            // For Quit habits, no log means no relapse (clean day)
            if (tracker.Type == HabitType.Quit)
            {
                return new HabitDaySummary { Date = date, IsCompleted = true, Target = 0, Value = "Clean" };
            }

            return new HabitDaySummary { Date = date, IsCompleted = false, Target = tracker.Target };
        }

        // Snapshot of target: use the first log's target snapshot if available, otherwise habit's current target
        double effectiveTarget = dayLogs.First().TargetSnapshot ?? tracker.Target;
        bool isAtLeast = tracker.Direction == HabitDirection.AtLeast;

        switch (tracker.Type)
        {
            case HabitType.Boolean:
            {
                var isDone = dayLogs.Any(l => l.Value.Equals("true", StringComparison.OrdinalIgnoreCase) || l.BooleanValue);
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = isAtLeast ? isDone : !isDone,
                    Target = 1,
                    NumericValue = isDone ? 1 : 0,
                    Value = isDone ? "true" : "false"
                };
            }

            case HabitType.Counter:
            {
                // Counter: sum numeric values or count of clicks
                double total = dayLogs.Sum(l => l.NumericValue ?? 1);
                bool completed = isAtLeast ? (effectiveTarget <= 0 ? total > 0 : total >= effectiveTarget)
                                           : total <= effectiveTarget;
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = completed,
                    Target = effectiveTarget,
                    NumericValue = total,
                    Value = total.ToString(CultureInfo.InvariantCulture)
                };
            }

            case HabitType.Quantity:
            case HabitType.Duration:
            {
                double total = dayLogs.Sum(l => l.NumericValue ?? 0);
                bool completed = isAtLeast ? total >= effectiveTarget : total <= effectiveTarget;
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = completed,
                    Target = effectiveTarget,
                    NumericValue = total,
                    Value = total.ToString("0.##", CultureInfo.InvariantCulture)
                };
            }

            case HabitType.Rating:
            {
                var latest = dayLogs.OrderByDescending(l => l.LoggedAt).First();
                double rating = latest.NumericValue ?? (double.TryParse(latest.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) ? r : 0);
                bool completed = isAtLeast ? rating >= effectiveTarget : rating <= effectiveTarget;
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = completed,
                    Target = effectiveTarget,
                    NumericValue = rating,
                    Value = rating.ToString("0.#", CultureInfo.InvariantCulture)
                };
            }

            case HabitType.TimeOfDay:
            {
                var latest = dayLogs.OrderByDescending(l => l.LoggedAt).First();
                bool completed = false;
                if (TimeOnly.TryParse(latest.Value, out var loggedTime))
                {
                    // Target can be formatted as time e.g. "07:00" in TypeConfig or numeric minutes in Target
                    TimeOnly targetTime = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(effectiveTarget));
                    completed = isAtLeast ? loggedTime >= targetTime : loggedTime <= targetTime;
                }
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = completed,
                    Target = effectiveTarget,
                    Value = latest.Value
                };
            }

            case HabitType.Checklist:
            {
                var latest = dayLogs.OrderByDescending(l => l.LoggedAt).First();
                // Value stores comma or json list of checked items
                int checkedCount = 0;
                if (!string.IsNullOrWhiteSpace(latest.Value))
                {
                    checkedCount = latest.Value.Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
                }
                bool completed = isAtLeast ? checkedCount >= effectiveTarget : checkedCount <= effectiveTarget;
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = completed,
                    Target = effectiveTarget,
                    NumericValue = checkedCount,
                    Value = latest.Value
                };
            }

            case HabitType.Choice:
            {
                var latest = dayLogs.OrderByDescending(l => l.LoggedAt).First();
                bool hasChoice = !string.IsNullOrWhiteSpace(latest.Value);
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = hasChoice,
                    Target = effectiveTarget,
                    Value = latest.Value
                };
            }

            case HabitType.Text:
            {
                var latest = dayLogs.OrderByDescending(l => l.LoggedAt).First();
                bool hasText = !string.IsNullOrWhiteSpace(latest.Value);
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = hasText,
                    Target = effectiveTarget,
                    Value = latest.Value
                };
            }

            default:
            {
                var latest = dayLogs.OrderByDescending(l => l.LoggedAt).First();
                return new HabitDaySummary
                {
                    Date = date,
                    IsCompleted = true,
                    Target = effectiveTarget,
                    NumericValue = latest.NumericValue,
                    Value = latest.Value
                };
            }
        }
    }

    private static TimeZoneInfo GetTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId) || timeZoneId.Equals("UTC", StringComparison.OrdinalIgnoreCase))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        }
        catch
        {
            return TimeZoneInfo.Utc;
        }
    }

    public static DateOnly GetLocalDate(DateTime utcDateTime, TimeZoneInfo timeZone)
    {
        var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Kind == DateTimeKind.Utc ? utcDateTime : utcDateTime.ToUniversalTime(), timeZone);
        return DateOnly.FromDateTime(localDateTime);
    }
}
