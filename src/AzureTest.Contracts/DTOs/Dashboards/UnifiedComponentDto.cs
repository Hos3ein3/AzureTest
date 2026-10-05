using System;

namespace AzureTest.Contracts.DTOs.Dashboards;

public class UnifiedComponentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Standard, RawHtml, JsonConfig
    public string Category { get; set; } = string.Empty;
    public string RequiredRole { get; set; } = string.Empty;
    public bool IsRaw { get; set; }
}
