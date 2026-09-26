using System;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Events.Commands;

public record CreateEventCommand(string Title, string? Description, DateTime StartDate, DateTime EndDate, bool IsAllDay, string? Color) : ICommand<Guid>;
