using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Commands;

public class LogHabitCommandHandler : BaseCommandHandler<LogHabitCommand, Result>
{
    private readonly ICurrentUser _currentUser;

    public LogHabitCommandHandler(ICurrentUser currentUser, IUnitOfWork unitOfWork, ILogger<LogHabitCommandHandler> logger)
        : base(unitOfWork, logger)
    {
        _currentUser = currentUser;
    }

    protected override async Task<Result> ExecuteAsync(LogHabitCommand command, CancellationToken cancellationToken)
    {
        var habit = await UnitOfWork.Habits.GetByIdAsync(command.HabitId);
        if (habit == null || habit.UserId != _currentUser.Id) return Result.Failure("Habit not found.");

        // Determine local date using habit's TimeZone if not provided
        var localDate = command.LocalDate ?? HabitStatsCalculator.GetLocalDate(DateTime.UtcNow, GetTimeZone(habit.TimeZone));
        var utcNow = DateTime.UtcNow;

        // Determine value string and numeric value
        string valueStr;
        double? numericVal = command.NumericValue;

        if (!string.IsNullOrWhiteSpace(command.Value))
        {
            valueStr = command.Value;
            if (!numericVal.HasValue && double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed))
            {
                numericVal = parsed;
            }
        }
        else if (command.BooleanValue.HasValue)
        {
            valueStr = command.BooleanValue.Value ? "true" : "false";
            numericVal ??= command.BooleanValue.Value ? 1.0 : 0.0;
        }
        else if (command.NumericValue.HasValue)
        {
            valueStr = command.NumericValue.Value.ToString("0.##", CultureInfo.InvariantCulture);
        }
        else
        {
            valueStr = "1";
            numericVal = 1.0;
        }

        // Logging semantics:
        // Counter is event-based: one log per click
        if (habit.Type == HabitType.Counter)
        {
            var counterLog = HabitLog.Create(
                habitTrackerId: habit.Id,
                userId: _currentUser.Id,
                localDate: localDate,
                loggedAt: utcNow,
                value: valueStr,
                numericValue: numericVal ?? 1.0,
                notes: command.Notes,
                targetSnapshot: habit.Target
            );

            UnitOfWork.HabitLogs.Add(counterLog);
        }
        else
        {
            // Boolean, Rating, Quantity, etc.: one log per day (upsert)
            var existingLog = await UnitOfWork.HabitLogs.GetHabitLogByDateAsync(habit.Id, localDate);
            if (existingLog != null)
            {
                existingLog.Update(valueStr, numericVal, command.Notes, utcNow);
                if (!existingLog.TargetSnapshot.HasValue)
                {
                    existingLog.SetTargetSnapshot(habit.Target);
                }
            }
            else
            {
                var newLog = HabitLog.Create(
                    habitTrackerId: habit.Id,
                    userId: _currentUser.Id,
                    localDate: localDate,
                    loggedAt: utcNow,
                    value: valueStr,
                    numericValue: numericVal,
                    notes: command.Notes,
                    targetSnapshot: habit.Target
                );

                UnitOfWork.HabitLogs.Add(newLog);
            }
        }

        return Result.Success();
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
}
