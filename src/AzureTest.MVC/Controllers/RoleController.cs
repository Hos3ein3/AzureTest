using AzureTest.Application.Abstractions;


using System.Threading.Tasks;
using AzureTest.Application.Users;
using AzureTest.Application.Users.Commands.CreateRole;
using AzureTest.Application.Users.Queries.GetAllRoles;
using AzureTest.MVC.ViewModels.Roles;
using AzureTest.Application.Permissions;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Role")]
[Description("Manage and control Role operations.")]
    public class RoleController : Controller
{
    private readonly IUserQueryUnit _queryUnit;
    private readonly IUserCommandUnit _commandUnit;
    private readonly IPermissionQueryUnit _permQueryUnit;
    private readonly IPermissionCommandUnit _permCommandUnit;

    public RoleController(IUserQueryUnit queryUnit, IUserCommandUnit commandUnit, IPermissionQueryUnit permQueryUnit, IPermissionCommandUnit permCommandUnit)
    {
        _queryUnit = queryUnit;
        _commandUnit = commandUnit;
        _permQueryUnit = permQueryUnit;
        _permCommandUnit = permCommandUnit;
    }

    // GET: Role
    [DisplayName("Index")]
    [Description("View a list of Role.")]
    public async Task<IActionResult> Index()
    {
        var roles = await _queryUnit.GetAllRoles.HandleAsync(new GetAllRolesQuery());
        return View(new RoleListViewModel { Roles = roles });
    }

    // GET: Role/Create
    [DisplayName("Create")]
    [Description("Create a new Role.")]
        public async Task<IActionResult> Create()
    {
        var allPermissions = await _permQueryUnit.GetPermissions.HandleAsync(new GetPermissionsQuery());
        var model = new RoleCreateViewModel
        {
            AvailablePermissions = allPermissions.OrderBy(p => p.ControllerName).ThenBy(p => p.ActionName).ToList()
        };
        return View(model);
    }

    // POST: Role/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Role.")]
        public async Task<IActionResult> Create(RoleCreateViewModel model)
    {
        if (!ModelState.IsValid) 
        {
            var allPermissions = await _permQueryUnit.GetPermissions.HandleAsync(new GetPermissionsQuery());
            model.AvailablePermissions = allPermissions.OrderBy(p => p.ControllerName).ThenBy(p => p.ActionName).ToList();
            return View(model);
        }

        var command = new CreateRoleCommand(model.RoleName);
        var result = await _commandUnit.CreateRole.HandleAsync(command);

        if (result.Success)
        {
            // Now assign selected permissions
            if (model.SelectedPermissions != null && model.SelectedPermissions.Any())
            {
                foreach(var permId in model.SelectedPermissions)
                {
                    await _permCommandUnit.AssignRolePermission.HandleAsync(new AssignRolePermissionCommand(model.RoleName, permId));
                }
            }

            TempData["Success"] = "Role created successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error);
        }

        var permsFallback = await _permQueryUnit.GetPermissions.HandleAsync(new GetPermissionsQuery());
        model.AvailablePermissions = permsFallback.OrderBy(p => p.ControllerName).ThenBy(p => p.ActionName).ToList();
        return View(model);
    }
}
