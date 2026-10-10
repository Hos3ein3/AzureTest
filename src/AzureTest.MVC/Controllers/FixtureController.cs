using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.FootballFantasy;
using AzureTest.Application.FootballFantasy.Commands;
using AzureTest.Application.FootballFantasy.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Fixture")]
[Description("Manage real-world football matches and fixtures.")]
public class FixtureController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public FixtureController(IFootballFantasyQueryUnit queries, IFootballFantasyCommandUnit commands, IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    private async Task PopulateViewBagsAsync(CancellationToken cancellationToken)
    {
        var teamsResult = await _queries.GetTeams.HandleAsync(new GetTeamsQuery(new TeamFilter { PageSize = 100 }), cancellationToken);
        var adminDash = await _queries.GetAdminDashboard.HandleAsync(new GetFantasyAdminDashboardQuery(), cancellationToken);
        ViewBag.Teams = teamsResult.Items;
        ViewBag.Leagues = adminDash.IsSuccess ? adminDash.Value.RecentLeagues : null;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("List football fixtures and match results with status filter.")]
    public async Task<IActionResult> Index(FixtureFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetFixtures.HandleAsync(new GetFixturesQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View fixture details, scores, and lineup.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetFixtureById.HandleAsync(new GetFixtureByIdQuery(id), cancellationToken);
        return result.ToViewResult(this);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Schedule a new football fixture.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateViewBagsAsync(cancellationToken);
        return View(new CreateFixtureCommand(Guid.NewGuid(), Guid.Empty, Guid.Empty, DateTime.UtcNow.AddDays(7)));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Schedule a new football fixture.")]
    public async Task<IActionResult> Create(CreateFixtureCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreateFixture.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Edit an existing fixture schedule or venue.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetFixtureById.HandleAsync(new GetFixtureByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        await PopulateViewBagsAsync(cancellationToken);
        var model = new UpdateFixtureCommand(
            result.Value.Id,
            result.Value.KickoffAt,
            result.Value.Round,
            null,
            result.Value.Venue,
            null,
            result.Value.Status
        );
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Update an existing fixture schedule.")]
    public async Task<IActionResult> Edit(UpdateFixtureCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.UpdateFixture.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Score")]
    [Description("Record score, minute, and status for a fixture.")]
    public async Task<IActionResult> UpdateScore(UpdateFixtureScoreCommand model, CancellationToken cancellationToken)
    {
        var result = await _commands.UpdateFixtureScore.HandleAsync(model, cancellationToken);
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a football fixture.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeleteFixture.HandleAsync(new DeleteFixtureCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
