using System.Collections.Generic;
using AzureTest.Application.Events.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Events;

namespace AzureTest.Application.Events;

public interface IEventQueryUnit
{
    IQueryHandler<GetAllEventsQuery, IReadOnlyList<EventDto>> GetAllEvents { get; }
    IQueryHandler<GetEventByIdQuery, EventDto?> GetEventById { get; }
}
