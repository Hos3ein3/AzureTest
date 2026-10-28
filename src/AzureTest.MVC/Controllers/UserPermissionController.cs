using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Permissions;
using AzureTest.Application.UserPermissions;
using AzureTest.Application.UserPermissions.Commands.UpdateUserPermissions;
using AzureTest.Application.UserPermissions.Queries.GetUserPermissions;
using AzureTest.Application.Users;
using AzureTest.Application.Users.Queries.GetUserById;
using AzureTest.MVC.ViewModels.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("User Permissions")]
[Description("Manage direct user permissions that override role-based access.")]
public class UserPermissionController : Controller
{
    private readonly IUserPermissionQueryUnit _userPermissionQuery;
    private readonly IUserPermissionCommandUnit _userPermissionCommand;
    private readonly IPermissionQueryUnit _permissionQuery;
    private readonly IUserQueryUnit _userQuery;
    private readonly IPermissionScanner _scanner;

    public UserPermissionController(
        IUserPermissionQueryUnit userPermissionQuery,
        IUserPermissionCommandUnit userPermissionCommand,
        IPermissionQueryUnit permissionQuery,
        IUserQueryUnit userQuery,
        IPermissionScanner scanner)
    {
        _userPermissionQuery = userPermissionQuery ?? throw new ArgumentNullException(nameof(userPermissionQuery));
        _userPermissionCommand = userPermissionCommand ?? throw new ArgumentNullException(nameof(userPermissionCommand));
        _permissionQuery = permissionQuery ?? throw new ArgumentNullException(nameof(permissionQuery));
        _userQuery = userQuery ?? throw new ArgumentNullException(nameof(userQuery));
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
    }

    [HttpGet]
    [DisplayName("User Permissions")]
    [Description("View and configure permissions assigned directly to a user.")]
    public async Task<IActionResult> Index(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("Invalid User ID.");
        }

        var user = await _userQuery.GetUserById.HandleAsync(new GetUserByIdQuery(userId), ct);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Index", "User");
        }

        var allPermissionsResult = await _permissionQuery.GetPermissions.HandleAsync(
            new GetPermissionsQuery(new PermissionFilter { PageSize = -1 }), ct);
        var allPermissions = allPermissionsResult.Items;

        var userPermissionsResult = await _userPermissionQuery.GetUserPermissions.HandleAsync(
            new GetUserPermissionsQuery(userId), ct);
        var assignedIds = userPermissionsResult.IsSuccess && userPermissionsResult.Value != null
            ? userPermissionsResult.Value.Where(up => up.IsGranted).Select(up => up.PermissionId).ToHashSet()
            : new HashSet<Guid>();

        var rolesList = (user.Roles ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .ToList();

        var roleAssignedIds = new HashSet<Guid>();
        foreach (var role in rolesList)
        {
            var rolePerms = await _permissionQuery.GetRolePermissions.HandleAsync(new GetRolePermissionsQuery(role), ct);
            if (rolePerms != null)
            {
                foreach (var rp in rolePerms)
                {
                    roleAssignedIds.Add(rp.PermissionId);
                }
            }
        }

        var adminOnlyEndpoints = _scanner.GetAdminOnlyEndpoints();

        var permissionItems = allPermissions.Select(p =>
        {
            var fullKey = !string.IsNullOrEmpty(p.AreaName) ? $"{p.AreaName}/{p.ControllerName}/{p.ActionName}" : $"{p.ControllerName}/{p.ActionName}";
            var shortKey = $"{p.ControllerName}/{p.ActionName}";
            var requiresAdmin = adminOnlyEndpoints.Contains(fullKey) || adminOnlyEndpoints.Contains(shortKey);

            return new UserPermissionItemViewModel
            {
                PermissionId = p.Id,
                AreaName = p.AreaName,
                ControllerName = p.ControllerName,
                ActionName = p.ActionName,
                DisplayName = p.DisplayName,
                Description = p.Description,
                IsRestricted = p.IsRestricted,
                IsAssigned = assignedIds.Contains(p.Id),
                IsGrantedViaRole = roleAssignedIds.Contains(p.Id),
                RequiresAdmin = requiresAdmin
            };
        }).OrderBy(p => p.AreaName).ThenBy(p => p.ControllerName).ThenBy(p => p.ActionName).ToList();

        var viewModel = new UserPermissionsViewModel
        {
            UserId = userId,
            UserName = !string.IsNullOrWhiteSpace(user.FullName) ? user.FullName : user.Email,
            UserEmail = user.Email,
            UserRoles = rolesList,
            Permissions = permissionItems
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update User Permissions")]
    [Description("Save direct user permission overrides.")]
    public async Task<IActionResult> Update(Guid userId, List<Guid> selectedPermissions, CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest("Invalid User ID.");
        }

        selectedPermissions ??= new List<Guid>();

        var result = await _userPermissionCommand.UpdateUserPermissions.HandleAsync(
            new UpdateUserPermissionsCommand(userId, selectedPermissions), ct);

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "User direct permissions updated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = result.Message ?? "Failed to update user permissions.";
        }

        return RedirectToAction(nameof(Index), new { userId });
    }
}
