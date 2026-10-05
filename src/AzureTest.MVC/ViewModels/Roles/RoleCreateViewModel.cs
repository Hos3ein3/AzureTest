using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AzureTest.Application.Permissions;

namespace AzureTest.MVC.ViewModels.Roles;

public class RoleCreateViewModel
{
    [Required]
    [Display(Name = "Role Name")]
    [StringLength(50, MinimumLength = 2)]
    public string RoleName { get; set; } = null!;

    [Required]
    [Display(Name = "Priority")]
    [Range(0, 1000)]
    public int Priority { get; set; } = 0;
    
    public List<Guid> SelectedPermissions { get; set; } = new();
    public List<PermissionDto> AvailablePermissions { get; set; } = new();
}
