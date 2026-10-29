using System.ComponentModel;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Permissions;
using Microsoft.AspNetCore.Identity;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Identity Settings")]
[Description("Manage authentication and identity configurations.")]
public class IdentitySettingController : Controller
{
    private readonly GetIdentitySettingQueryHandler _getQuery;
    private readonly UpdateIdentitySettingCommandHandler _updateCommand;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IPermissionQueryUnit _permissionQuery;

    public IdentitySettingController(
        GetIdentitySettingQueryHandler getQuery, 
        UpdateIdentitySettingCommandHandler updateCommand,
        RoleManager<ApplicationRole> roleManager,
        IPermissionQueryUnit permissionQuery)
    {
        _getQuery = getQuery;
        _updateCommand = updateCommand;
        _roleManager = roleManager;
        _permissionQuery = permissionQuery;
    }

    [HttpGet]
    [DisplayName("Get Role Permissions")]
    [Description("Load permissions assigned to a role in JSON format.")]
    public async Task<IActionResult> GetRolePermissionsJson(string roleName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return Json(new object[] { });

        var perms = await _permissionQuery.GetRolePermissions.HandleAsync(new GetRolePermissionsQuery(roleName), cancellationToken);
        var data = perms.Select(p => new
        {
            area = p.Permission.AreaName ?? "",
            controller = p.Permission.ControllerName,
            action = p.Permission.ActionName,
            displayName = p.Permission.DisplayName,
            description = p.Permission.Description,
            isRestricted = p.Permission.IsRestricted
        }).ToList();

        return Json(data);
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Action: Edit")]
    public async Task<IActionResult> Edit()
    {
        var result = await _getQuery.HandleAsync(new GetIdentitySettingQuery());
        if (!result.IsSuccess || result.Value == null)
            return NotFound();

        ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();
        return View(result.Value);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Action: Edit")]
    public async Task<IActionResult> Edit(AzureTest.Domain.Documents.IdentitySetting model)
    {
        if (ModelState.IsValid)
        {
            var command = new UpdateIdentitySettingCommand(
                model.Id,
                model.RedirectAfterRegister,
                model.RedirectAfterLogin,
                model.RolesForRegister,
                model.StayLoginAfterRegister,
                model.LogoutOtherDevicesOnLogin
            );

            var result = await _updateCommand.HandleAsync(command);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Identity Settings updated successfully.";
                return RedirectToAction(nameof(Edit));
            }
            ModelState.AddModelError("", result.Error ?? "Failed to update identity settings.");
        }
        
        ViewBag.Roles = _roleManager.Roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name }).ToList();
        return View(model);
    }
}
