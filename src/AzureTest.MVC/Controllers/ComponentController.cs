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
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
public class ComponentController : Controller
{
    private readonly IQueryHandler<GetAllComponentsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<DashboardComponent>> _getAllComponentsQuery;
    private readonly IQueryHandler<GetComponentByIdQuery, DashboardComponent?> _getComponentByIdQuery;
    private readonly ICommandHandler<CreateDashboardComponentCommand> _createCommand;
    private readonly ICommandHandler<UpdateDashboardComponentCommand> _updateCommand;
    private readonly ICommandHandler<DeleteDashboardComponentCommand> _deleteCommand;
    private readonly IQueryHandler<GetAllRawComponentsQuery, IReadOnlyList<DashboardRawComponent>> _getAllRawComponentsQuery;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ComponentController(
        IQueryHandler<GetAllComponentsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<DashboardComponent>> getAllComponentsQuery,
        IQueryHandler<GetComponentByIdQuery, DashboardComponent?> getComponentByIdQuery,
        ICommandHandler<CreateDashboardComponentCommand> createCommand,
        ICommandHandler<UpdateDashboardComponentCommand> updateCommand,
        ICommandHandler<DeleteDashboardComponentCommand> deleteCommand,
        IQueryHandler<GetAllRawComponentsQuery, IReadOnlyList<DashboardRawComponent>> getAllRawComponentsQuery,
        IWebHostEnvironment webHostEnvironment)
    {
        _getAllComponentsQuery = getAllComponentsQuery;
        _getComponentByIdQuery = getComponentByIdQuery;
        _createCommand = createCommand;
        _updateCommand = updateCommand;
        _deleteCommand = deleteCommand;
        _getAllRawComponentsQuery = getAllRawComponentsQuery;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken, string? searchTerm = null, string? category = null, string? sortBy = null, bool sortDescending = false, int pageNumber = 1, int pageSize = 10)
    {
        var query = new GetAllComponentsQuery(searchTerm, category, sortBy, sortDescending, pageNumber, pageSize);
        var components = await _getAllComponentsQuery.HandleAsync(query, cancellationToken);
        
        ViewBag.SearchTerm = searchTerm;
        ViewBag.Category = category;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDescending = sortDescending;
        
        var rawComponents = await _getAllRawComponentsQuery.HandleAsync(new GetAllRawComponentsQuery(), cancellationToken);
        ViewBag.RawComponents = rawComponents;
        
        return View(components);
    }

    public IActionResult Create(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new DashboardComponent { DefaultWidth = 4, DefaultHeight = 4 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            model.DefaultHeight
        );

        await _createCommand.HandleAsync(cmd, cancellationToken);
        TempData["Success"] = "Component created successfully.";
        
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var component = await _getComponentByIdQuery.HandleAsync(new GetComponentByIdQuery(id), cancellationToken);
        if (component == null) return NotFound();

        return View(component);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            model.DefaultHeight
        );

        await _updateCommand.HandleAsync(cmd, cancellationToken);
        TempData["Success"] = "Component updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteCommand.HandleAsync(new DeleteDashboardComponentCommand(id), cancellationToken);
        TempData["Success"] = "Component deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
