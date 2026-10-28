using System;
using System.Collections.Generic;

namespace AzureTest.MVC.ViewModels.Permissions;

public class UserPermissionsViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public List<string> UserRoles { get; set; } = new();
    public List<UserPermissionItemViewModel> Permissions { get; set; } = new();
}

public class UserPermissionItemViewModel
{
    public Guid PermissionId { get; set; }
    public string? AreaName { get; set; } = string.Empty;
    public string ControllerName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRestricted { get; set; }
    public bool IsAssigned { get; set; }
    public bool IsGrantedViaRole { get; set; }
    public bool RequiresAdmin { get; set; }
}
