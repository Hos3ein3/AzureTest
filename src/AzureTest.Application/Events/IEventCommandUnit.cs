using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Events.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Events;

public interface IEventCommandUnit
{
    ICommandHandler<CreateEventCommand, Guid> CreateEvent { get; }
    ICommandHandler<UpdateEventCommand, Result> UpdateEvent { get; }
    ICommandHandler<DeleteEventCommand, Result> DeleteEvent { get; }
}
