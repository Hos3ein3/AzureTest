using System.ComponentModel;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AzureTest.Application.IdentitySettings;
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

    public IdentitySettingController(
        GetIdentitySettingQueryHandler getQuery, 
        UpdateIdentitySettingCommandHandler updateCommand,
        RoleManager<ApplicationRole> roleManager)
    {
        _getQuery = getQuery;
        _updateCommand = updateCommand;
        _roleManager = roleManager;
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
                model.StayLoginAfterRegister
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
