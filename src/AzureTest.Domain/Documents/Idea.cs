using System;
using System.Collections.Generic;

namespace AzureTest.Domain.Documents;

public enum IdeaStatus { Inbox = 0, Refinement = 1, InProgress = 2, Completed = 3, Archived = 4 }
public enum IdeaPriority { Low = 0, Medium = 1, High = 2, Urgent = 3 }

public class IdeaTask
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class Idea
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; set; }
    public Guid? ParentId { get; set; } // Notion allows infinite nesting
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // Markdown or HTML
    
    // Notion-like aesthetic properties
    public string Icon { get; set; } = "fa-regular fa-lightbulb"; 
    public string CoverImage { get; set; } = string.Empty; 
    public string ThemeColor { get; set; } = "blue"; // brand, blue, emerald, rose, etc.

    public IdeaStatus Status { get; set; } = IdeaStatus.Inbox;
    public IdeaPriority Priority { get; set; } = IdeaPriority.Medium;
    
    public List<string> Tags { get; set; } = new();
    public List<IdeaTask> Tasks { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
