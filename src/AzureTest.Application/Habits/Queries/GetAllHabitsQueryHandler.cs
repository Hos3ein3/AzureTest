using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Habits;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Queries;

public class GetAllHabitsQueryHandler : BaseQueryHandler<GetAllHabitsQuery, IReadOnlyList<HabitTrackerDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllHabitsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllHabitsQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<IReadOnlyList<HabitTrackerDto>> ExecuteAsync(GetAllHabitsQuery query, CancellationToken cancellationToken)
    {
        var habits = await _unitOfWork.Habits.GetHabitsWithLogsAsync(query.UserId);

        return habits.Select(h =>
        {
            var activeLogs = h.Logs.Where(l => !l.IsDeleted).ToList();
            var stats = HabitStatsCalculator.Calculate(h, activeLogs);

            var logDtos = activeLogs
                .OrderByDescending(l => l.LocalDate)
                .ThenByDescending(l => l.LoggedAt)
                .Select(l => new HabitLogDto(
                    l.Id,
                    l.HabitTrackerId,
                    l.UserId,
                    l.LocalDate,
                    l.LoggedAt,
                    l.Value,
                    l.NumericValue ?? 0,
                    l.Notes,
                    l.TargetSnapshot
                )).ToList();

            return new HabitTrackerDto(
                h.Id,
                h.Name,
                h.Description,
                h.Type,
                h.Direction,
                h.Target,
                h.Schedule,
                h.Frequency,
                h.TimeZone,
                h.IsArchived,
                h.IsPrivate,
                h.TypeConfig,
                h.UserId,
                logDtos,
                stats.CurrentStreak,
                stats.LongestStreak,
                stats.IsCompletedToday,
                stats.TodayNumericValue,
                stats.TodayValue,
                stats.ProgressPercentage
            );
        }).ToList();
    }
}
