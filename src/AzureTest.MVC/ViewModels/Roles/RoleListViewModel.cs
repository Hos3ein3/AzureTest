using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Users;

namespace AzureTest.MVC.ViewModels.Roles;

public class RoleListViewModel
{
    public IReadOnlyList<RoleDto> Roles { get; set; } = new List<RoleDto>();
}
