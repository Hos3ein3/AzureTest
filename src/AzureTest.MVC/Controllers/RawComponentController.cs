using AzureTest.Application.Abstractions;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Dashboards.Queries;
using AzureTest.Application.Dashboards.RawComponents;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Raw Component")]
[Description("Manage and control Raw Component operations.")]
    public class RawComponentController : Controller
{
    private readonly IQueryHandler<GetUserDashboardsQuery, IEnumerable<UserDashboard>> _getDashboardsQuery;
    private readonly IQueryHandler<GetRawComponentByIdQuery, DashboardRawComponent?> _getByIdQuery;
    private readonly ICommandHandler<CreateRawComponentCommand, Result<Guid>> _createCommand;
    private readonly ICommandHandler<UpdateRawComponentCommand> _updateCommand;
    private readonly ICommandHandler<DeleteRawComponentCommand> _deleteCommand;
    private readonly ICurrentUser _currentUser;

    public RawComponentController(
        IQueryHandler<GetUserDashboardsQuery, IEnumerable<UserDashboard>> getDashboardsQuery,
        IQueryHandler<GetRawComponentByIdQuery, DashboardRawComponent?> getByIdQuery,
        ICommandHandler<CreateRawComponentCommand, Result<Guid>> createCommand,
        ICommandHandler<UpdateRawComponentCommand> updateCommand,
        ICommandHandler<DeleteRawComponentCommand> deleteCommand,
        ICurrentUser currentUser)
    {
        _getDashboardsQuery = getDashboardsQuery;
        _getByIdQuery = getByIdQuery;
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _deleteCommand = deleteCommand;
        _currentUser = currentUser;
    }
    
    [HttpGet]
    [DisplayName("Index")]
    [Description("View a list of Raw Component.")]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Component");
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new Raw Component.")]
    public IActionResult Create(string? type, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewBag.ReturnUrl = returnUrl;
        var model = new DashboardRawComponent { Type = type ?? "RawHtml" };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Raw Component.")]
    public async Task<IActionResult> Create(DashboardRawComponent model, string? returnUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError("", "Please fill in all required fields.");
        }

        if (model.Type == "RawHtml" && string.IsNullOrWhiteSpace(model.HtmlContent))
            ModelState.AddModelError("HtmlContent", "HTML Content is required for RawHtml type.");
            
        if (model.Type == "JsonConfig" && string.IsNullOrWhiteSpace(model.JsonConfiguration))
            ModelState.AddModelError("JsonConfiguration", "JSON Configuration is required for JsonConfig type.");

        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var command = new CreateRawComponentCommand(
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.ViewName,
            model.Category,
            model.RequiredRole ?? "",
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );

        await _createCommand.HandleAsync(command, cancellationToken);
        
        TempData["Success"] = "Dynamic component successfully added.";
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction("Index", "Component");
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Modify an existing Raw Component.")]
    public async Task<IActionResult> Edit(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
        var component = await _getByIdQuery.HandleAsync(new GetRawComponentByIdQuery(id), cancellationToken);
        if (component == null) return NotFound();

        ViewBag.ReturnUrl = returnUrl;
        return View(component);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing Raw Component.")]
    public async Task<IActionResult> Edit(Guid id, DashboardRawComponent model, string? returnUrl, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();

        if (string.IsNullOrWhiteSpace(model.Name))
            ModelState.AddModelError("", "Please fill in all required fields.");

        if (model.Type == "RawHtml" && string.IsNullOrWhiteSpace(model.HtmlContent))
            ModelState.AddModelError("HtmlContent", "HTML Content is required for RawHtml type.");
            
        if (model.Type == "JsonConfig" && string.IsNullOrWhiteSpace(model.JsonConfiguration))
            ModelState.AddModelError("JsonConfiguration", "JSON Configuration is required for JsonConfig type.");

        if (!ModelState.IsValid)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        var command = new UpdateRawComponentCommand(
            model.Id,
            model.Name,
            model.Type,
            model.HtmlContent,
            model.JsonConfiguration,
            model.ViewName,
            model.Category,
            model.RequiredRole ?? "",
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );

        await _updateCommand.HandleAsync(command, cancellationToken);
        
        TempData["Success"] = "Dynamic component successfully updated.";
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
            
        return RedirectToAction("Index", "Component");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a Raw Component.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteCommand.HandleAsync(new DeleteRawComponentCommand(id), cancellationToken);
        TempData["Success"] = "Dynamic component removed.";
        return RedirectToAction("Index", "Component");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(Guid id, CancellationToken cancellationToken)
    {
        var component = await _getByIdQuery.HandleAsync(new GetRawComponentByIdQuery(id), cancellationToken);
        if (component == null) return NotFound();

        var createCmd = new CreateRawComponentCommand(
            component.Name + " - Copy",
            component.Type,
            component.HtmlContent,
            component.JsonConfiguration,
            component.ViewName,
            component.Category,
            component.RequiredRole,
            component.MinWidth,
            component.MaxWidth,
            component.MinHeight,
            component.MaxHeight,
            component.IsResizable
        );
        
        var result = await _createCommand.HandleAsync(createCmd, cancellationToken);
        if (result.IsFailure)
        {
            TempData["Error"] = "Failed to duplicate component: " + (result.Message ?? result.Error);
            return RedirectToAction(nameof(Index), "Component");
        }

        TempData["Success"] = "Component duplicated successfully. You are now editing the copy.";
        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }
}
