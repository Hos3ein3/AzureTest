using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Events;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Events.Queries;

public class GetEventByIdQueryHandler : BaseQueryHandler<GetEventByIdQuery, EventDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEventByIdQueryHandler(IUnitOfWork unitOfWork, ILogger<GetEventByIdQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<EventDto?> ExecuteAsync(GetEventByIdQuery query, CancellationToken cancellationToken)
    {
        var e = await _unitOfWork.Events.GetByIdAsync(query.Id);
        if (e == null || e.UserId != query.UserId) return null;
        
        return new EventDto(e.Id, e.Title, e.Description, e.StartDate, e.EndDate, e.IsAllDay, e.Color, e.UserId);
    }
}
