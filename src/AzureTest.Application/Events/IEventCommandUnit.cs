using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Events.Commands;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Events;

public interface IEventCommandUnit
{
    ICommandHandler<CreateEventCommand, Result<Guid>> CreateEvent { get; }
    ICommandHandler<UpdateEventCommand, Result> UpdateEvent { get; }
    ICommandHandler<DeleteEventCommand, Result> DeleteEvent { get; }
}
