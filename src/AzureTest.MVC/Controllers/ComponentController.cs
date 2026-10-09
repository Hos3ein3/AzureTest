using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Dashboards;
using AzureTest.Application.Abstractions;


using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Dashboards.Components.Commands;
using AzureTest.Application.Dashboards.Components.Queries;
using AzureTest.Application.Dashboards.RawComponents;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Component")]
[Description("Manage and control Component operations.")]
    public class ComponentController : Controller
{
    private readonly IQueryHandler<GetAllComponentsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<AzureTest.Contracts.DTOs.Dashboards.UnifiedComponentDto>> _getAllComponentsQuery;
    private readonly IQueryHandler<GetComponentByIdQuery, DashboardComponent?> _getComponentByIdQuery;
    private readonly ICommandHandler<CreateDashboardComponentCommand, Result<Guid>> _createCommand;
    private readonly ICommandHandler<UpdateDashboardComponentCommand> _updateCommand;
    private readonly ICommandHandler<DeleteDashboardComponentCommand> _deleteCommand;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ComponentController(
        IQueryHandler<GetAllComponentsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<AzureTest.Contracts.DTOs.Dashboards.UnifiedComponentDto>> getAllComponentsQuery,
        IQueryHandler<GetComponentByIdQuery, DashboardComponent?> getComponentByIdQuery,
        ICommandHandler<CreateDashboardComponentCommand, Result<Guid>> createCommand,
        ICommandHandler<UpdateDashboardComponentCommand> updateCommand,
        ICommandHandler<DeleteDashboardComponentCommand> deleteCommand,
        IWebHostEnvironment webHostEnvironment)
    {
        _getAllComponentsQuery = getAllComponentsQuery;
        _getComponentByIdQuery = getComponentByIdQuery;
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _deleteCommand = deleteCommand;
        _webHostEnvironment = webHostEnvironment;
    }

    [DisplayName("Index")]
    [Description("View a list of Component.")]
    public async Task<IActionResult> Index([FromQuery] AzureTest.Contracts.DTOs.Dashboards.GetAllComponentsFilters filters, [FromQuery] string? search, [FromQuery] string? category, [FromQuery] string? type, [FromQuery] string? sort, [FromQuery] string? dir, [FromQuery] int? page, CancellationToken cancellationToken = default)
    {
        filters ??= new AzureTest.Contracts.DTOs.Dashboards.GetAllComponentsFilters();
        if (!string.IsNullOrWhiteSpace(search)) filters.SearchTerm = search;
        if (!string.IsNullOrWhiteSpace(category)) filters.Category = category;
        if (!string.IsNullOrWhiteSpace(type)) filters.Type = type;
        if (!string.IsNullOrWhiteSpace(sort)) filters.SortBy = sort;
        if (!string.IsNullOrWhiteSpace(dir)) filters.SortDescending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
        if (page.HasValue && page.Value > 0) filters.PageNumber = page.Value;

        var query = new GetAllComponentsQuery(filters);
        var components = await _getAllComponentsQuery.HandleAsync(query, cancellationToken);

        var pagedWithFilters = new PagedResult<UnifiedComponentDto, AzureTest.Contracts.DTOs.Dashboards.GetAllComponentsFilters>(
            components.Items, components.TotalCount, components.PageNumber, components.PageSize, filters);

        var allCats = components.Items.Select(c => c.Category ?? "General").Distinct().OrderBy(c => c).ToList();
        ViewBag.AllCategories = allCats;
        ViewBag.Filters = filters;

        return View(pagedWithFilters);
    }

    [DisplayName("Create")]
    [Description("Create a new Component.")]
    public IActionResult Create(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new DashboardComponent { DefaultWidth = 4, DefaultHeight = 4 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Component.")]
    public async Task<IActionResult> Create(DashboardComponent model, IFormFile? imageFile, string? returnUrl, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) 
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        if (imageFile != null && imageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "dashboardsamples");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            model.ImagePath = $"/dashboardsamples/{uniqueFileName}";
        }

        var cmd = new CreateDashboardComponentCommand(
            model.Name,
            model.Description ?? "",
            model.ImagePath ?? "",
            model.Category ?? "General",
            model.RequiredRole ?? "",
            model.PartialViewName,
            model.DefaultWidth,
            model.DefaultHeight,
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );

        await _createCommand.HandleAsync(cmd, cancellationToken);
        TempData["Success"] = "Component created successfully.";
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction(nameof(Index));
    }

    [DisplayName("Edit")]
    [Description("Modify an existing Component.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var component = await _getComponentByIdQuery.HandleAsync(new GetComponentByIdQuery(id), cancellationToken);
        if (component == null) return NotFound();

        return View(component);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing Component.")]
    public async Task<IActionResult> Edit(DashboardComponent model, IFormFile? imageFile, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        if (imageFile != null && imageFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "dashboardsamples");
            Directory.CreateDirectory(uploadsFolder);
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }
            model.ImagePath = $"/dashboardsamples/{uniqueFileName}";
        }
        else if (string.IsNullOrEmpty(model.ImagePath))
        {
            // preserve existing if not provided
            var existing = await _getComponentByIdQuery.HandleAsync(new GetComponentByIdQuery(model.Id), cancellationToken);
            if (existing != null) model.ImagePath = existing.ImagePath;
        }

        var cmd = new UpdateDashboardComponentCommand(
            model.Id,
            model.Name,
            model.Description ?? "",
            model.ImagePath ?? "",
            model.Category ?? "General",
            model.RequiredRole ?? "",
            model.PartialViewName,
            model.DefaultWidth,
            model.DefaultHeight,
            model.MinWidth,
            model.MaxWidth,
            model.MinHeight,
            model.MaxHeight,
            model.IsResizable
        );

        await _updateCommand.HandleAsync(cmd, cancellationToken);
        TempData["Success"] = "Component updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a Component.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteCommand.HandleAsync(new DeleteDashboardComponentCommand(id), cancellationToken);
        TempData["Success"] = "Component deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(Guid id, CancellationToken cancellationToken)
    {
        var component = await _getComponentByIdQuery.HandleAsync(new GetComponentByIdQuery(id), cancellationToken);
        if (component == null) return NotFound();

        var createCmd = new CreateDashboardComponentCommand(
            component.Name + " - Copy",
            component.Description,
            component.ImagePath,
            component.Category,
            component.RequiredRole,
            component.PartialViewName,
            component.DefaultWidth,
            component.DefaultHeight,
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
            return RedirectToAction(nameof(Index));
        }

        TempData["Success"] = "Component duplicated successfully. You are now editing the copy.";
        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }
}
