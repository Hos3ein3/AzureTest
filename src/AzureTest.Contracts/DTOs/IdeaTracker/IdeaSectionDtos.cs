using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.IdeaTracker;

public record IdeaSectionDto(
    Guid Id,
    Guid IdeaPageId,
    string Title,
    string Content,
    int OrderIndex,
    DateTime CreatedAt,
    int X,
    int Y,
    int Width,
    int Height,
    DateTime? UpdatedAt
);

public class CreateIdeaSectionDto : IIdeaSectionFormDto
{
    public Guid IdeaPageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class UpdateIdeaSectionDto : IIdeaSectionFormDto
{
    public Guid Id { get; set; }
    public Guid IdeaPageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public interface IIdeaSectionFormDto
{
    Guid IdeaPageId { get; set; }
    string Title { get; set; }
    string Content { get; set; }
    int OrderIndex { get; set; }
}
