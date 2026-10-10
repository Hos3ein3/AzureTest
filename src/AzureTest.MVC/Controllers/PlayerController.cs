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
[DisplayName("Player")]
[Description("Manage real-world football players.")]
public class PlayerController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public PlayerController(IFootballFantasyQueryUnit queries, IFootballFantasyCommandUnit commands, IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    private async Task PopulateViewBagsAsync(CancellationToken cancellationToken)
    {
        var countriesResult = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 100 }), cancellationToken);
        var teamsResult = await _queries.GetTeams.HandleAsync(new GetTeamsQuery(new TeamFilter { PageSize = 100 }), cancellationToken);
        ViewBag.Countries = countriesResult.Items;
        ViewBag.Teams = teamsResult.Items;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("List real players with position, price, and stats.")]
    public async Task<IActionResult> Index(PlayerFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPlayers.HandleAsync(new GetPlayersQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View player profile, bio, FUT card, and fantasy stats.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPlayerById.HandleAsync(new GetPlayerByIdQuery(id), cancellationToken);
        return result.ToViewResult(this);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new football player.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateViewBagsAsync(cancellationToken);
        return View(new CreatePlayerCommand(string.Empty, string.Empty, string.Empty, Guid.Empty, PlayerPosition.Midfielder));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new football player.")]
    public async Task<IActionResult> Create(CreatePlayerCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreatePlayer.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Edit an existing football player.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPlayerById.HandleAsync(new GetPlayerByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        await PopulateViewBagsAsync(cancellationToken);
        var model = new UpdatePlayerCommand(
            result.Value.Id,
            result.Value.FirstName,
            result.Value.LastName,
            result.Value.DisplayName,
            result.Value.CountryId,
            result.Value.Position,
            result.Value.DateOfBirth,
            result.Value.PreferredFoot,
            result.Value.HeightCm,
            result.Value.WeightKg,
            result.Value.PhotoUrl,
            result.Value.IsActive
        );
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Update an existing football player.")]
    public async Task<IActionResult> Edit(UpdatePlayerCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.UpdatePlayer.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a football player.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeletePlayer.HandleAsync(new DeletePlayerCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
