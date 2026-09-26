using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Events;
using AzureTest.Application.Interfaces.Data;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Events.Queries;

public class GetAllEventsQueryHandler : BaseQueryHandler<GetAllEventsQuery, IReadOnlyList<EventDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllEventsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetAllEventsQueryHandler> logger) : base(logger)
    {
        _unitOfWork = unitOfWork;
    }

    protected override async Task<IReadOnlyList<EventDto>> ExecuteAsync(GetAllEventsQuery query, CancellationToken cancellationToken)
    {
        var events = await _unitOfWork.Events.GetEventsForUserAsync(query.UserId);

        return events.OrderBy(e => e.StartDate).Select(e => new EventDto(
            e.Id, e.Title, e.Description, e.StartDate, e.EndDate, e.IsAllDay, e.Color, e.UserId
        )).ToList();
    }
}
