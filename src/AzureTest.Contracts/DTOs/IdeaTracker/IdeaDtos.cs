using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.IdeaTracker;

public record IdeaTaskDto(Guid Id, string Description, bool IsCompleted);

public record IdeaDto(
    Guid Id,
    Guid UserId,
    Guid? ParentId,
    string Title,
    string Content,
    string Icon,
    string CoverImage,
    string ThemeColor,
    int Status,
    int Priority,
    List<string> Tags,
    List<IdeaTaskDto> Tasks,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public class CreateIdeaDto
{
    public Guid? ParentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-regular fa-lightbulb";
    public string CoverImage { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = "blue";
    public int Status { get; set; } = 0;
    public int Priority { get; set; } = 1;
    public List<string> Tags { get; set; } = new();
}

public class UpdateIdeaDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string CoverImage { get; set; } = string.Empty;
    public string ThemeColor { get; set; } = string.Empty;
    public int Status { get; set; }
    public int Priority { get; set; }
    public List<string> Tags { get; set; } = new();
}
