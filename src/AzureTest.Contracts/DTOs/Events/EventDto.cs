using System;

namespace AzureTest.Contracts.DTOs.Events;

public record EventDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAllDay,
    string? Color,
    Guid UserId
);

public record CreateEventDto(
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAllDay,
    string? Color
);

public record UpdateEventDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsAllDay,
    string? Color
);
