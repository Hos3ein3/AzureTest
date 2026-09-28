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
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class RawComponentController : Controller
{
    private readonly IQueryHandler<GetUserDashboardsQuery, IEnumerable<UserDashboard>> _getDashboardsQuery;
    private readonly IQueryHandler<GetRawComponentByIdQuery, DashboardRawComponent?> _getByIdQuery;
    private readonly ICommandHandler<CreateRawComponentCommand> _createCommand;
    private readonly ICommandHandler<UpdateRawComponentCommand> _updateCommand;
    private readonly ICommandHandler<DeleteRawComponentCommand> _deleteCommand;
    private readonly ICurrentUser _currentUser;

    public RawComponentController(
        IQueryHandler<GetUserDashboardsQuery, IEnumerable<UserDashboard>> getDashboardsQuery,
        IQueryHandler<GetRawComponentByIdQuery, DashboardRawComponent?> getByIdQuery,
        ICommandHandler<CreateRawComponentCommand> createCommand,
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
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Component");
    }

    [HttpGet]
    public IActionResult Create(string? type, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewBag.ReturnUrl = returnUrl;
        var model = new DashboardRawComponent { Type = type ?? "RawHtml" };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            model.Category,
            model.RequiredRole
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
    public async Task<IActionResult> Edit(Guid id, string? returnUrl, CancellationToken cancellationToken)
    {
        var component = await _getByIdQuery.HandleAsync(new GetRawComponentByIdQuery(id), cancellationToken);
        if (component == null) return NotFound();

        ViewBag.ReturnUrl = returnUrl;
        return View(component);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            model.Category,
            model.RequiredRole
        );

        await _updateCommand.HandleAsync(command, cancellationToken);
        
        TempData["Success"] = "Dynamic component successfully updated.";
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
            
        return RedirectToAction("Index", "Component");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteCommand.HandleAsync(new DeleteRawComponentCommand(id), cancellationToken);
        TempData["Success"] = "Dynamic component removed.";
        return RedirectToAction("Index", "Component");
    }
}
