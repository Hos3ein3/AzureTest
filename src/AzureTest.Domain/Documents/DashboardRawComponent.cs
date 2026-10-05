using System;

namespace AzureTest.Domain.Documents;

public class DashboardRawComponent
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "RawHtml" or "JsonConfig"
    public string? HtmlContent { get; set; }
    public string? JsonConfiguration { get; set; }
    public string? ViewName { get; set; }
    public string? Category { get; set; } = string.Empty;
    public string? RequiredRole { get; set; }
    public int MinWidth { get; set; }
    public int MaxWidth { get; set; }
    public int MinHeight { get; set; }
    public int MaxHeight { get; set; }
    public bool IsResizable { get; set; } = true;
}
