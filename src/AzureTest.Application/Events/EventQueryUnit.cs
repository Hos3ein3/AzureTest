using System.Collections.Generic;
using AzureTest.Application.Events.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Events;

namespace AzureTest.Application.Events;

public class EventQueryUnit : IEventQueryUnit
{
    public IQueryHandler<GetAllEventsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<EventDto, GetAllEventsFilters>> GetAllEvents { get; }
    public IQueryHandler<GetEventByIdQuery, EventDto?> GetEventById { get; }

    public EventQueryUnit(
        IQueryHandler<GetAllEventsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<EventDto, GetAllEventsFilters>> getAllEvents,
        IQueryHandler<GetEventByIdQuery, EventDto?> getEventById)
    {
        GetAllEvents = getAllEvents;
        GetEventById = getEventById;
    }
}
