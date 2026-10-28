using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Permission Management")]
[Description("Manage system permissions.")]
public class PermissionController : Controller
{
    private readonly IPermissionQueryUnit _queryUnit;
    private readonly IPermissionCommandUnit _commandUnit;
    private readonly IPermissionScanner _scanner;
    private readonly IRedisService _redisService;

    public PermissionController(
        IPermissionQueryUnit queryUnit,
        IPermissionCommandUnit commandUnit,
        IPermissionScanner scanner, IRedisService redisService)
    {
        _queryUnit = queryUnit;
        _commandUnit = commandUnit;
        _scanner = scanner;
        _redisService = redisService;
    }

    
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string controllerName, string actionName, string displayName, string description, string? areaName = null)
    {
        var command = new CreatePermissionCommand(controllerName, actionName, displayName, description, areaName);
        var result = await _commandUnit.CreatePermission.HandleAsync(command);
        
        if (result.IsSuccess)
        {
            await _redisService.RemoveAsync("AllPermissions");
            TempData["SuccessMessage"] = "Permission created successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        TempData["ErrorMessage"] = result.Message ?? "Failed to create permission.";
        return View();
    }

    [DisplayName("Index")]
    [Description("View a list of Permissions.")]
    public async Task<IActionResult> Index([FromQuery] PermissionFilter? filter, CancellationToken cancellationToken = default)
    {
        filter ??= new PermissionFilter();

        var paged = await _queryUnit.GetPermissions.HandleAsync(new GetPermissionsQuery(filter), cancellationToken);

        ViewBag.CurrentPage = paged.PageNumber;
        ViewBag.TotalPages = paged.TotalPages;
        ViewBag.PageSize = paged.PageSize;
        ViewBag.TotalItems = paged.TotalCount;
        ViewBag.Sort = filter.SortBy;
        ViewBag.Dir = filter.SortDescending ? "desc" : "asc";
        ViewBag.Search = filter.SearchTerm ?? "";
        ViewBag.Filters = filter;

        return View(paged.Items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sync()
    {
        var discovered = _scanner.ScanPermissions();
        var command = new SyncPermissionsCommand(discovered);
        var result = await _commandUnit.SyncPermissions.HandleAsync(command);

        if (result.IsSuccess)
        {
            await _redisService.RemoveAsync("AllPermissions");
            TempData["SuccessMessage"] = "Permissions synced successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to sync permissions.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRestriction(Guid id, bool isRestricted)
    {
        var command = new TogglePermissionRestrictionCommand(id, isRestricted);
        var result = await _commandUnit.TogglePermissionRestriction.HandleAsync(command);
        
                if (result.IsSuccess)
        {
            await _redisService.RemoveAsync("AllPermissions");
            TempData["SuccessMessage"] = "Restriction toggled.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to toggle.";
        }
            
        return RedirectToAction(nameof(Index));
    }

    
    public async Task<IActionResult> RolePermissions(string roleName)
    {
        var allPermissionsResult = await _queryUnit.GetPermissions.HandleAsync(new GetPermissionsQuery(new PermissionFilter { PageSize = -1 }));
        var allPermissions = allPermissionsResult.Items;
        var assignedPermissions = await _queryUnit.GetRolePermissions.HandleAsync(new GetRolePermissionsQuery(roleName));
        var assignedIds = System.Linq.Enumerable.ToHashSet(System.Linq.Enumerable.Select(assignedPermissions, rp => rp.PermissionId));
        var adminOnlyEndpoints = _scanner.GetAdminOnlyEndpoints();

        var model = new AzureTest.MVC.ViewModels.Permissions.RolePermissionsViewModel
        {
            RoleName = roleName,
            Permissions = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderBy(System.Linq.Enumerable.Select(allPermissions, p => {
                var fullKey = !string.IsNullOrEmpty(p.AreaName) ? $"{p.AreaName}/{p.ControllerName}/{p.ActionName}" : $"{p.ControllerName}/{p.ActionName}";
                var shortKey = $"{p.ControllerName}/{p.ActionName}";
                var requiresAdmin = adminOnlyEndpoints.Contains(fullKey) || adminOnlyEndpoints.Contains(shortKey);

                return new AzureTest.MVC.ViewModels.Permissions.RolePermissionItemViewModel
                {
                    PermissionId = p.Id,
                    AreaName = p.AreaName,
                    ControllerName = p.ControllerName,
                    ActionName = p.ActionName,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                    IsRestricted = p.IsRestricted,
                    IsAssigned = assignedIds.Contains(p.Id),
                    RequiresAdmin = requiresAdmin
                };
            }), p => p.AreaName).ThenBy(p => p.ControllerName).ThenBy(p => p.ActionName))
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRolePermissions(string roleName, System.Collections.Generic.List<Guid> selectedPermissions)
    {
        var assignedPermissions = await _queryUnit.GetRolePermissions.HandleAsync(new GetRolePermissionsQuery(roleName));
        var currentlyAssigned = System.Linq.Enumerable.ToHashSet(System.Linq.Enumerable.Select(assignedPermissions, rp => rp.PermissionId));
        selectedPermissions = selectedPermissions ?? new System.Collections.Generic.List<Guid>();

        // Add new
        foreach(var id in selectedPermissions)
        {
            if(!currentlyAssigned.Contains(id))
            {
                await _commandUnit.AssignRolePermission.HandleAsync(new AssignRolePermissionCommand(roleName, id));
            }
        }

        // Remove old
        foreach(var id in currentlyAssigned)
        {
            if(!selectedPermissions.Contains(id))
            {
                await _commandUnit.RemoveRolePermission.HandleAsync(new RemoveRolePermissionCommand(roleName, id));
            }
        }

        await _redisService.RemoveAsync($"RolePermissions_{roleName}");
        TempData["SuccessMessage"] = "Role permissions updated successfully.";
        return RedirectToAction(nameof(RolePermissions), new { roleName });
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRolePermission(string roleName, Guid permissionId)
    {
        var result = await _commandUnit.AssignRolePermission.HandleAsync(new AssignRolePermissionCommand(roleName, permissionId));
        return RedirectToAction(nameof(RolePermissions), new { roleName });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveRolePermission(string roleName, Guid permissionId)
    {
        var result = await _commandUnit.RemoveRolePermission.HandleAsync(new RemoveRolePermissionCommand(roleName, permissionId));
        return RedirectToAction(nameof(RolePermissions), new { roleName });
    }
}
