using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class IdeaPage : ITenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid IdeaId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    
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
