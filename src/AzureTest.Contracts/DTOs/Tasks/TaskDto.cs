using System;

namespace AzureTest.Contracts.DTOs.Tasks;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime? DueDate,
    bool IsCompleted,
    string? Priority,
    Guid UserId
);
