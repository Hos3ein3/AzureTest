using System;
using System.Collections.Generic;

namespace AzureTest.Domain.Documents;

public class UserDashboard
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
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
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
