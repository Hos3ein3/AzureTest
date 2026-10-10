using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.FootballFantasy;
using AzureTest.Application.FootballFantasy.Commands;
using AzureTest.Application.FootballFantasy.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Achievements")]
[Description("Manage Football Fantasy achievements, milestones and badge awards.")]
public class AchievementController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public AchievementController(
        IFootballFantasyQueryUnit queries,
        IFootballFantasyCommandUnit commands,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    [HttpGet]
    [DisplayName("Achievement List")]
    [Description("View all fantasy game achievements with filtering and pagination.")]
    public async Task<IActionResult> Index(AchievementFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetAchievements.HandleAsync(new GetAchievementsQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Achievement Details")]
    [Description("View single achievement reward details.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetAchievementById.HandleAsync(new GetAchievementByIdQuery(id), cancellationToken);
        return result.ToViewResult(this);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [DisplayName("Create Achievement")]
    [Description("Open creation form to define a new fantasy achievement.")]
    public IActionResult Create()
    {
        var model = new CreateAchievementCommand(string.Empty, string.Empty, "bi-trophy-fill", 100);
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    [DisplayName("Create Achievement")]
    [Description("Submit new achievement reward.")]
    public async Task<IActionResult> Create(CreateAchievementCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure) return validation.ToFormResult(this, model, nameof(Index));

        var result = await _commands.CreateAchievement.HandleAsync(model, cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [DisplayName("Edit Achievement")]
    [Description("Open form to edit existing achievement parameters.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetAchievementById.HandleAsync(new GetAchievementByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        var model = new UpdateAchievementCommand(
            result.Value.Id,
            result.Value.Title,
            result.Value.Description,
            result.Value.BadgeIcon,
            result.Value.PointsReward,
            result.Value.IsActive
        );
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit Achievement")]
    [Description("Save changes to an existing achievement.")]
    public async Task<IActionResult> Edit(UpdateAchievementCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure) return validation.ToFormResult(this, model, nameof(Index));

        var result = await _commands.UpdateAchievement.HandleAsync(model, cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Achievement")]
    [Description("Delete an achievement reward.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeleteAchievement.HandleAsync(new DeleteAchievementCommand(id), cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess ? "Achievement removed." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
