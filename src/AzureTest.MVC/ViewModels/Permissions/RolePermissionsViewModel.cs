using System;
using System.Collections.Generic;

namespace AzureTest.MVC.ViewModels.Permissions;

public class RolePermissionsViewModel
{
    public string RoleName { get; set; } = string.Empty;
    public List<RolePermissionItemViewModel> Permissions { get; set; } = new();
}

public class RolePermissionItemViewModel
{
    public Guid PermissionId { get; set; }
    public string? AreaName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRestricted { get; set; }
    public bool IsAssigned { get; set; }
    public bool RequiresAdmin { get; set; }
}
