

using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class UserDashboard : ITenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<DashboardSection> Sections { get; set; } = new();
}

public class DashboardSection
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<UserDashboardComponent> Components { get; set; } = new();
}

public class UserDashboardComponent
{
    public Guid ComponentId { get; set; }
    public Guid? RawComponentId { get; set; }
    public string Type { get; set; } = "Standard"; // "Standard", "RawHtml", "JsonConfig"
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
