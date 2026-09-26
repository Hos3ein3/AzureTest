using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Habits;
using AzureTest.Application.Interfaces.Data;
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
        
        return new HabitTrackerDto(
            h.Id, h.Name, h.Description, h.Type, h.Frequency, h.Goal, h.UserId,
            h.Logs.OrderByDescending(l => l.LogDate).Select(l => new HabitLogDto(
                l.Id, l.LogDate, l.BooleanValue, l.NumericValue, l.Notes
            )).ToList()
        );
    }
}
