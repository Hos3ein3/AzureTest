using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Habits;
using AzureTest.Application.Interfaces.Data;
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

        return habits.Select(h => new HabitTrackerDto(
            h.Id, h.Name, h.Description, h.Type, h.Frequency, h.Goal, h.UserId,
            h.Logs.OrderByDescending(l => l.LogDate).Select(l => new HabitLogDto(
                l.Id, l.LogDate, l.BooleanValue, l.NumericValue, l.Notes
            )).ToList()
        )).ToList();
    }
}
