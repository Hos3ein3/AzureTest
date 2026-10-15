using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities;

public class Permission : BaseEntity
{
    public string? AreaName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Description { get; set; } = string.Empty;
    public bool IsRestricted { get; set; }
}
