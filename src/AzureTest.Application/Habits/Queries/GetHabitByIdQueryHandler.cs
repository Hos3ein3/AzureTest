using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Contracts.DTOs.Habits;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Habits.Queries;

public class GetHabitByIdQueryHandler : BaseQueryHandler<GetHabitByIdQuery, HabitTrackerDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetHabitByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetHabitByIdQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<HabitTrackerDto?> ExecuteAsync(GetHabitByIdQuery query, CancellationToken cancellationToken)
    {
        var h = await _unitOfWork.Habits.GetHabitWithLogsByIdAsync(query.Id, query.UserId);
        if (h == null) return null;

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
    }
}
