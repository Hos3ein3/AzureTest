using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Events.Commands;

public record UpdateEventCommand(Guid Id, string Title, string? Description, DateTime StartDate, DateTime EndDate, bool IsAllDay, string? Color) : ICommand<Result>;
