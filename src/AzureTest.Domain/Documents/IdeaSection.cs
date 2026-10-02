using System;

namespace AzureTest.Domain.Documents;

public class IdeaSection
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid IdeaPageId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    
    
    // Layout
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 4;

    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
