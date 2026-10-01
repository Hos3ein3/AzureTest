

using System;

namespace AzureTest.Domain.Documents;

public class DashboardComponent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string RequiredRole { get; set; } = string.Empty;
    public string PartialViewName { get; set; } = string.Empty;
    public int DefaultWidth { get; set; }
    public int DefaultHeight { get; set; }
}
