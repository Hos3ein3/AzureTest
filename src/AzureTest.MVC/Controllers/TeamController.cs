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
[DisplayName("Team")]
[Description("Manage real-world football clubs and national teams.")]
public class TeamController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public TeamController(IFootballFantasyQueryUnit queries, IFootballFantasyCommandUnit commands, IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    private async Task PopulateViewBagsAsync(CancellationToken cancellationToken)
    {
        var countriesResult = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 100 }), cancellationToken);
        ViewBag.Countries = countriesResult.Items;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("List real football clubs with stadium and country info.")]
    public async Task<IActionResult> Index(TeamFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetTeams.HandleAsync(new GetTeamsQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View team details, stadium, and roster.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetTeamById.HandleAsync(new GetTeamByIdQuery(id), cancellationToken);
        return result.ToViewResult(this);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new football team.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateViewBagsAsync(cancellationToken);
        return View(new CreateTeamCommand(string.Empty, string.Empty, TeamType.Club, Guid.Empty));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new football team.")]
    public async Task<IActionResult> Create(CreateTeamCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreateTeam.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Edit an existing football team.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetTeamById.HandleAsync(new GetTeamByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        await PopulateViewBagsAsync(cancellationToken);
        var model = new UpdateTeamCommand(
            result.Value.Id,
            result.Value.Name,
            result.Value.ShortName,
            result.Value.TeamType,
            result.Value.CountryId,
            result.Value.Code,
            result.Value.FoundedYear,
            result.Value.StadiumName,
            result.Value.StadiumCapacity,
            result.Value.LogoUrl
        );
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Update an existing football team.")]
    public async Task<IActionResult> Edit(UpdateTeamCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.UpdateTeam.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a football team.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeleteTeam.HandleAsync(new DeleteTeamCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
