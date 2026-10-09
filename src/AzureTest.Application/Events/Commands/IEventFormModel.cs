using System;

namespace AzureTest.Application.Events.Commands;

public interface IEventFormModel
{
    string Title { get; }
    string? Description { get; }
    DateTime StartDate { get; }
    DateTime EndDate { get; }
    bool IsAllDay { get; }
    string? Color { get; }
}
