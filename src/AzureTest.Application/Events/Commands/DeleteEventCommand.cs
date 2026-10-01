using AzureTest.Application.Abstractions;

using System;

using AzureTest.Application.Messaging;

namespace AzureTest.Application.Events.Commands;

public record DeleteEventCommand(Guid Id) : ICommand<Result>;
