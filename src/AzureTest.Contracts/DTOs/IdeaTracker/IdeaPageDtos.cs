using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.IdeaTracker;

public record IdeaPageDto(
    Guid Id,
    Guid IdeaId,
    string Title,
    string Content,
    DateTime CreatedAt,
    int X,
    int Y,
    int Width,
    int Height,
    DateTime? UpdatedAt
);

public class CreateIdeaPageDto
{
    public Guid IdeaId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class UpdateIdeaPageDto
{
    public Guid Id { get; set; }
    public Guid IdeaId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
