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
[DisplayName("League")]
[Description("Manage real-world football competitions and leagues.")]
public class LeagueController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public LeagueController(IFootballFantasyQueryUnit queries, IFootballFantasyCommandUnit commands, IValidationService validator)
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
    [Description("List real football leagues with filtering and pagination.")]
    public async Task<IActionResult> Index(LeagueFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetLeagues.HandleAsync(new GetLeaguesQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View league details and active season status.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetLeagueById.HandleAsync(new GetLeagueByIdQuery(id), cancellationToken);
        return result.ToViewResult(this);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new football league.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateViewBagsAsync(cancellationToken);
        return View(new CreateLeagueCommand(string.Empty, LeagueType.League, LeagueLevel.Domestic));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new football league.")]
    public async Task<IActionResult> Create(CreateLeagueCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreateLeague.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Edit an existing football league.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetLeagueById.HandleAsync(new GetLeagueByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        await PopulateViewBagsAsync(cancellationToken);
        var model = new UpdateLeagueCommand(
            result.Value.Id,
            result.Value.Name,
            result.Value.Type,
            result.Value.Level,
            result.Value.ShortName,
            null,
            result.Value.Continent,
            result.Value.LogoUrl,
            result.Value.IsActive
        );
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Update an existing football league.")]
    public async Task<IActionResult> Edit(UpdateLeagueCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.UpdateLeague.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a football league.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeleteLeague.HandleAsync(new DeleteLeagueCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
