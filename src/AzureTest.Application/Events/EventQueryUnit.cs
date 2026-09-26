using System.Collections.Generic;
using AzureTest.Application.Events.Queries;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Events;

namespace AzureTest.Application.Events;

public class EventQueryUnit : IEventQueryUnit
{
    public IQueryHandler<GetAllEventsQuery, IReadOnlyList<EventDto>> GetAllEvents { get; }
    public IQueryHandler<GetEventByIdQuery, EventDto?> GetEventById { get; }

    public EventQueryUnit(
        IQueryHandler<GetAllEventsQuery, IReadOnlyList<EventDto>> getAllEvents,
        IQueryHandler<GetEventByIdQuery, EventDto?> getEventById)
    {
        GetAllEvents = getAllEvents;
        GetEventById = getEventById;
    }
}
