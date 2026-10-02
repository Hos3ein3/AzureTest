using AzureTest.Application.Abstractions;


using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Application.Messaging;
using AzureTest.Application.Dashboards.Queries;
using AzureTest.Application.Dashboards.Commands;
using AzureTest.Application.Dashboards.RawComponents;
using AzureTest.Application.UserPreferences.Commands;
using AzureTest.Application.UserPreferences.Queries;
using AzureTest.Domain.Documents;
using AzureTest.MVC.Models;
using AzureTest.MVC.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Dashboard")]
[Description("Manage and control Dashboard operations.")]
    public class DashboardController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IQueryHandler<GetUserDashboardsQuery, IEnumerable<UserDashboard>> _getDashboardsHandler;
    private readonly IQueryHandler<GetUserDashboardBySlugQuery, UserDashboard?> _getBySlugHandler;
    private readonly IQueryHandler<GetDashboardComponentsQuery, IEnumerable<DashboardComponent>> _getComponentsHandler;
    private readonly ICommandHandler<CreateUserDashboardCommand> _createDashboardHandler;
    private readonly FluentValidation.IValidator<CreateUserDashboardCommand> _createValidator;
    private readonly FluentValidation.IValidator<UpdateUserDashboardCommand> _updateValidator;
    private readonly ICommandHandler<UpdateUserDashboardCommand> _updateDashboardHandler;
    private readonly ICommandHandler<DeleteUserDashboardCommand> _deleteDashboardHandler;
    private readonly IQueryHandler<GetUserPreferenceQuery, UserPreference> _getPreferenceHandler;
    private readonly ICommandHandler<UpdateUserPreferenceCommand> _updatePreferenceHandler;
    private readonly Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> _dashboardSettings;

    private readonly IQueryHandler<GetAllRawComponentsQuery, IReadOnlyList<DashboardRawComponent>> _getAllRawComponentsHandler;
    private readonly IDashboardDataLoader _dataLoader;

    public DashboardController(
        UserManager<ApplicationUser> userManager,
        IQueryHandler<GetUserDashboardsQuery, IEnumerable<UserDashboard>> getDashboardsHandler,
        IQueryHandler<GetUserDashboardBySlugQuery, UserDashboard?> getBySlugHandler,
        IQueryHandler<GetDashboardComponentsQuery, IEnumerable<DashboardComponent>> getComponentsHandler,
        ICommandHandler<CreateUserDashboardCommand> createDashboardHandler,
        FluentValidation.IValidator<CreateUserDashboardCommand> createValidator,
        FluentValidation.IValidator<UpdateUserDashboardCommand> updateValidator,
        ICommandHandler<UpdateUserDashboardCommand> updateDashboardHandler,
        ICommandHandler<DeleteUserDashboardCommand> deleteDashboardHandler,
        IQueryHandler<GetUserPreferenceQuery, UserPreference> getPreferenceHandler,
        ICommandHandler<UpdateUserPreferenceCommand> updatePreferenceHandler,
        Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings> dashboardSettings,

        IQueryHandler<GetAllRawComponentsQuery, IReadOnlyList<DashboardRawComponent>> getAllRawComponentsHandler,
        IDashboardDataLoader dataLoader)
    {
        _userManager = userManager;
        _getDashboardsHandler = getDashboardsHandler;
        _getBySlugHandler = getBySlugHandler;
        _getComponentsHandler = getComponentsHandler;
        _createDashboardHandler = createDashboardHandler;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _updateDashboardHandler = updateDashboardHandler;
        _deleteDashboardHandler = deleteDashboardHandler;
        _getPreferenceHandler = getPreferenceHandler;
        _updatePreferenceHandler = updatePreferenceHandler;
        _dashboardSettings = dashboardSettings;

        _getAllRawComponentsHandler = getAllRawComponentsHandler;
        _dataLoader = dataLoader;
    }

    [HttpGet]
    [DisplayName("Manage")]
    [Description("Execute Manage operation for Dashboard.")]
    public async Task<IActionResult> Manage(CancellationToken cancellationToken)
    {
        var userIdStr = _userManager.GetUserId(User);
        if (userIdStr == null) return RedirectToAction("Login", "Auth");
        var userId = Guid.Parse(userIdStr);

        var dashboards = await _getDashboardsHandler.HandleAsync(new GetUserDashboardsQuery(userId), cancellationToken);
        var pref = await _getPreferenceHandler.HandleAsync(new GetUserPreferenceQuery(userId), cancellationToken);
        ViewBag.PreferredDashboard = pref?.PreferredDashboard ?? string.Empty;
        
        return View(dashboards);
    }

    [HttpGet]
    [DisplayName("Builder")]
    [Description("Execute Builder operation for Dashboard.")]
    public async Task<IActionResult> Builder(string? slug, CancellationToken cancellationToken)
    {
        var userIdStr = _userManager.GetUserId(User);
        if (userIdStr == null) return RedirectToAction("Login", "Auth");
        var userId = Guid.Parse(userIdStr);
        
        var allComponents = await _getComponentsHandler.HandleAsync(new GetDashboardComponentsQuery(), cancellationToken);
        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin)
        {
            allComponents = allComponents.Where(c => string.IsNullOrEmpty(c.RequiredRole) || c.RequiredRole != "Admin");
        }

        UserDashboard? existing = null;
        if (!string.IsNullOrEmpty(slug))
        {
            existing = await _getBySlugHandler.HandleAsync(new GetUserDashboardBySlugQuery(userId, slug), cancellationToken);
            if (existing == null)
            {
                return NotFound();
            }
        }

        ViewBag.Components = allComponents.ToList();
        
        var allRawComponents = await _getAllRawComponentsHandler.HandleAsync(new GetAllRawComponentsQuery(), cancellationToken);
        if (!isAdmin)
        {
            allRawComponents = allRawComponents.Where(c => string.IsNullOrEmpty(c.RequiredRole) || c.RequiredRole != "Admin").ToList();
        }
        ViewBag.AllRawComponents = allRawComponents;
        
        return View(existing);
    }

    [HttpPost]
    [DisplayName("Save")]
    [Description("Execute Save operation for Dashboard.")]
    public async Task<IActionResult> Save([FromBody] SaveDashboardPayload payload, CancellationToken cancellationToken)
    {
        var userIdStr = _userManager.GetUserId(User);
        if (userIdStr == null) return Unauthorized();
        var userId = Guid.Parse(userIdStr);
        
        if (string.IsNullOrEmpty(payload.Name) || string.IsNullOrEmpty(payload.Slug))
            return BadRequest("Name and slug are required");

        try
        {
            if (string.IsNullOrEmpty(payload.Id) || payload.Id == Guid.Empty.ToString()) // create new
            {
                var cmd = new CreateUserDashboardCommand(userId, payload.Name, payload.Slug, payload.Sections);
                var validation = await _createValidator.ValidateAsync(cmd, cancellationToken);
                if (!validation.IsValid) return BadRequest(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
                await _createDashboardHandler.HandleAsync(cmd, cancellationToken);
            }
            else // update existing
            {
                var cmd = new UpdateUserDashboardCommand(Guid.Parse(payload.Id), userId, payload.Name, payload.Slug, payload.Sections);
                var validation = await _updateValidator.ValidateAsync(cmd, cancellationToken);
                if (!validation.IsValid) return BadRequest(string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));
                await _updateDashboardHandler.HandleAsync(cmd, cancellationToken);
            }

            return Ok(new { success = true, slug = payload.Slug });
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("duplicate key error"))
            {
                return BadRequest("A dashboard with this Name or Slug already exists. Please choose a different one.");
            }
            throw;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a Dashboard.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userIdStr = _userManager.GetUserId(User);
        if (userIdStr == null) return Unauthorized();
        var userId = Guid.Parse(userIdStr);

        var dashboardToDelete = await _getDashboardsHandler.HandleAsync(new GetUserDashboardsQuery(userId), cancellationToken);
        var dashboard = dashboardToDelete.FirstOrDefault(d => d.Id == id);
        
        await _deleteDashboardHandler.HandleAsync(new DeleteUserDashboardCommand(id, userId), cancellationToken);
        
        if (dashboard != null)
        {
            var pref = await _getPreferenceHandler.HandleAsync(new GetUserPreferenceQuery(userId), cancellationToken);
            if (pref.PreferredDashboard == dashboard.Slug)
            {
                var cmd = new UpdateUserPreferenceCommand(userId, pref.Language, pref.ThemeColor, pref.Currency, string.Empty);
                await _updatePreferenceHandler.HandleAsync(cmd, cancellationToken);
            }
        }

        return RedirectToAction("Manage");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Set Default")]
    [Description("Execute Set Default operation for Dashboard.")]
    public async Task<IActionResult> SetDefault(string slug, CancellationToken cancellationToken)
    {
        var userIdStr = _userManager.GetUserId(User);
        if (userIdStr == null) return Unauthorized();
        var userId = Guid.Parse(userIdStr);
        
        var pref = await _getPreferenceHandler.HandleAsync(new GetUserPreferenceQuery(userId), cancellationToken);
        var cmd = new UpdateUserPreferenceCommand(userId, pref?.Language ?? "", pref?.ThemeColor ?? "", pref?.Currency ?? "USD", slug ?? "");
        await _updatePreferenceHandler.HandleAsync(cmd, cancellationToken);
        
        return RedirectToAction("Manage");
    }

    [HttpGet]
    [DisplayName("Check Slug")]
    [Description("Execute Check Slug operation for Dashboard.")]
    public async Task<IActionResult> CheckSlug(string slug, Guid? excludeId, CancellationToken cancellationToken)
    {
        var userIdStr = _userManager.GetUserId(User);
        if (userIdStr == null) return Json(new { isAvailable = false });
        var userId = Guid.Parse(userIdStr);
        
        var dashboards = await _getDashboardsHandler.HandleAsync(new GetUserDashboardsQuery(userId), cancellationToken);
        var isTaken = dashboards.Any(d => string.Equals(d.Slug, slug, StringComparison.OrdinalIgnoreCase) && d.Id != excludeId);
        
        var isReserved = _dashboardSettings.Value.ReservedSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase);

        return Json(new { isAvailable = !isTaken && !isReserved });
    }

    [HttpGet]
    [DisplayName("View Dashboard")]
    [Description("View details of a Dashboard.")]
    public async Task<IActionResult> ViewDashboard(string slug, string? targetUserId, CancellationToken cancellationToken)
    {
        var currentUserIdStr = _userManager.GetUserId(User);
        if (currentUserIdStr == null) return RedirectToAction("Login", "Auth");
        
        var isAdmin = User.IsInRole("Admin");
        
        // If not admin, you can only view your own user id
        if (!isAdmin && !string.IsNullOrEmpty(targetUserId) && targetUserId != currentUserIdStr)
        {
            return Forbid();
        }

        // The dashboard layout comes from the CURRENT user's saved layouts, regardless of whose data they're viewing.
        var layoutOwnerId = Guid.Parse(currentUserIdStr);

        var dashboard = await _getBySlugHandler.HandleAsync(new GetUserDashboardBySlugQuery(layoutOwnerId, slug), cancellationToken);
        if (dashboard == null)
        {
            return NotFound();
        }

        var allComponents = await _getComponentsHandler.HandleAsync(new GetDashboardComponentsQuery(), cancellationToken);
        ViewBag.AllComponents = allComponents.ToDictionary(c => c.Id);

        var dataUserIdStr = string.IsNullOrWhiteSpace(targetUserId) && !isAdmin ? currentUserIdStr : targetUserId;

        // Load the shared view model data
        var homeModel = await _dataLoader.LoadDataAsync(User, dataUserIdStr, cancellationToken);
        
        var model = new DashboardViewModel
        {
            Dashboard = dashboard,
            HomeModel = homeModel
        };

        var rawComponents = await _getAllRawComponentsHandler.HandleAsync(new GetAllRawComponentsQuery(), cancellationToken);
        if (!isAdmin)
        {
            rawComponents = rawComponents.Where(c => string.IsNullOrEmpty(c.RequiredRole) || c.RequiredRole != "Admin").ToList();
        }
        ViewBag.AllRawComponents = rawComponents.ToDictionary(c => c.Id);
        
        return View(model);
    }
}

public class SaveDashboardPayload
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<DashboardSectionDto> Sections { get; set; } = new();
}

public class DashboardViewModel
{
    public UserDashboard Dashboard { get; set; } = null!;
    public HomeDashboardViewModel HomeModel { get; set; } = null!;
}
