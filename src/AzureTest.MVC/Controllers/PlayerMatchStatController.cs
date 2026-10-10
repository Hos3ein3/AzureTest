using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.FootballFantasy;
using AzureTest.Application.FootballFantasy.Commands;
using AzureTest.Application.FootballFantasy.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Player Match Stats")]
[Description("Manage real-world match performance statistics for players.")]
public class PlayerMatchStatController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly IValidationService _validator;

    public PlayerMatchStatController(IFootballFantasyQueryUnit queries, IFootballFantasyCommandUnit commands, IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _validator = validator;
    }

    private async Task PopulateViewBagsAsync(CancellationToken cancellationToken)
    {
        var playersResult = await _queries.GetPlayers.HandleAsync(new GetPlayersQuery(new PlayerFilter { PageSize = 100 }), cancellationToken);
        var teamsResult = await _queries.GetTeams.HandleAsync(new GetTeamsQuery(new TeamFilter { PageSize = 100 }), cancellationToken);
        var fixturesResult = await _queries.GetFixtures.HandleAsync(new GetFixturesQuery(new FixtureFilter { PageSize = 50 }), cancellationToken);
        ViewBag.Players = playersResult.Items;
        ViewBag.Teams = teamsResult.Items;
        ViewBag.Fixtures = fixturesResult.Items;
    }

    [HttpGet]
    [DisplayName("Index")]
    [Description("List recorded player stats for fixtures.")]
    public async Task<IActionResult> Index(PlayerMatchStatFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPlayerMatchStats.HandleAsync(new GetPlayerMatchStatsQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        return View(result);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View detailed match performance stats of a player.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPlayerMatchStatById.HandleAsync(new GetPlayerMatchStatByIdQuery(id), cancellationToken);
        return result.ToViewResult(this);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Record player match stats for a fixture.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateViewBagsAsync(cancellationToken);
        return View(new CreatePlayerMatchStatCommand(Guid.Empty, Guid.Empty, Guid.Empty, true, 90));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Record player match stats for a fixture.")]
    public async Task<IActionResult> Create(CreatePlayerMatchStatCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            await PopulateViewBagsAsync(cancellationToken);
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreatePlayerMatchStat.HandleAsync(model, cancellationToken);
        if (result.IsFailure) await PopulateViewBagsAsync(cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpGet]
    [DisplayName("Edit")]
    [Description("Edit recorded player match stats.")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPlayerMatchStatById.HandleAsync(new GetPlayerMatchStatByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        var s = result.Value;
        var model = new UpdatePlayerMatchStatCommand(
            s.Id,
            s.IsStarter,
            s.MinutesPlayed,
            s.Goals,
            s.Assists,
            s.OwnGoals,
            s.ShotsTotal,
            s.ShotsOnTarget,
            s.PassesTotal,
            s.KeyPasses,
            s.Tackles,
            s.Interceptions,
            s.Saves,
            s.GoalsConceded,
            s.PenaltiesSaved,
            s.PenaltiesMissed,
            s.YellowCards,
            s.RedCards,
            s.CleanSheet,
            s.ProviderRating
        );
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Update recorded player match stats.")]
    public async Task<IActionResult> Edit(UpdatePlayerMatchStatCommand model, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.UpdatePlayerMatchStat.HandleAsync(model, cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete player match stats.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.DeletePlayerMatchStat.HandleAsync(new DeletePlayerMatchStatCommand(id), cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
