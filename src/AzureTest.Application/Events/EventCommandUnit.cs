using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Events.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Events;

public class EventCommandUnit : IEventCommandUnit
{
    public ICommandHandler<CreateEventCommand, Guid> CreateEvent { get; }
    public ICommandHandler<UpdateEventCommand, Result> UpdateEvent { get; }
    public ICommandHandler<DeleteEventCommand, Result> DeleteEvent { get; }

    public EventCommandUnit(
        ICommandHandler<CreateEventCommand, Guid> createEvent,
        ICommandHandler<UpdateEventCommand, Result> updateEvent,
        ICommandHandler<DeleteEventCommand, Result> deleteEvent)
    {
        CreateEvent = createEvent;
        UpdateEvent = updateEvent;
        DeleteEvent = deleteEvent;
    }
}
