using System;

namespace AzureTest.Domain.Documents;

public class DashboardComponent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? RequiredRole { get; set; }
    public string PartialViewName { get; set; } = string.Empty;
    public int DefaultWidth { get; set; }
    public int DefaultHeight { get; set; }
    public int MinHeight { get; set; }
    public int MaxHeight { get; set; }
    public int MinWidth { get; set; }
    public int MaxWidth { get; set; }
    public bool IsResizable { get; set; } = true;
}
