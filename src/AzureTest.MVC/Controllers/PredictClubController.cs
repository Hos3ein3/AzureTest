using System;
using System.ComponentModel;
using System.Linq;
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
[DisplayName("Predict Club")]
[Description("Participate in Football Fantasy Predict Clubs, predicting match scores, facts, and tournament outcomes.")]
public class PredictClubController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly ICurrentUser _currentUser;
    private readonly IValidationService _validator;

    public PredictClubController(
        IFootballFantasyQueryUnit queries,
        IFootballFantasyCommandUnit commands,
        ICurrentUser currentUser,
        IValidationService validator)
    {
        _queries = queries;
        _commands = commands;
        _currentUser = currentUser;
        _validator = validator;
    }

    // GET: /PredictClub
    [HttpGet]
    [DisplayName("Predict Clubs")]
    [Description("Browse and search Predict Clubs across public, global, and your private joined clubs.")]
    public async Task<IActionResult> Index(PredictClubFilter filter, CancellationToken cancellationToken)
    {
        filter ??= new PredictClubFilter { PageSize = 12 };
        var result = await _queries.GetPredictClubs.HandleAsync(new GetPredictClubsQuery(filter), cancellationToken);

        ViewBag.Filter = filter;
        ViewBag.IsAdmin = _currentUser.IsAdmin;
        return View(result);
    }

    // GET: /PredictClub/Details/{id}
    [HttpGet]
    [DisplayName("Club Details")]
    [Description("View Predict Club leaderboard, upcoming fixtures, and your match prediction drafts.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPredictClubDetails.HandleAsync(new GetPredictClubDetailsQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        var details = result.Value;
        if (details.Club.Visibility == FantasyLeagueVisibility.Private && !string.IsNullOrWhiteSpace(details.Club.JoinCode))
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            ViewBag.InviteLink = $"{baseUrl}/PredictClub/Join/{id}?code={details.Club.JoinCode}";
        }

        // Teams & Players for tournament predictions
        var teamsResult = await _queries.GetTeams.HandleAsync(new GetTeamsQuery(new TeamFilter { PageSize = 100 }), cancellationToken);
        var playersResult = await _queries.GetPlayers.HandleAsync(new GetPlayersQuery(new PlayerFilter { PageSize = 150 }), cancellationToken);

        ViewBag.AvailableTeams = teamsResult.Items;
        ViewBag.AvailablePlayers = playersResult.Items;

        return View(details);
    }

    // GET: /PredictClub/Create
    [HttpGet]
    [DisplayName("Create Predict Club")]
    [Description("Create and configure a new Fantasy Predict Club with custom scoring and competition scope.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var allLeaguesResult = await _queries.GetLeagues.HandleAsync(new GetLeaguesQuery(new LeagueFilter { PageSize = 200 }), cancellationToken);
        var allCountriesResult = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 250 }), cancellationToken);

        ViewBag.AvailableLeagues = allLeaguesResult.Items;
        ViewBag.AvailableCountries = allCountriesResult.Items;

        var cmd = new CreatePredictClubCommand(
            Name: string.Empty,
            StartsAt: DateTime.UtcNow.AddDays(1),
            EndsAt: DateTime.UtcNow.AddMonths(1),
            Visibility: FantasyLeagueVisibility.Public,
            Format: FantasyLeagueFormat.TotalPoints,
            ScopeType: PoolScopeType.All,
            ScopeValue: "All"
        );

        return View(cmd);
    }

    // POST: /PredictClub/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create Predict Club (Submit)")]
    [Description("Handle creation and rule configuration of a new Fantasy Predict Club.")]
    public async Task<IActionResult> Create(CreatePredictClubCommand command, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            var allLeaguesResult = await _queries.GetLeagues.HandleAsync(new GetLeaguesQuery(new LeagueFilter { PageSize = 200 }), cancellationToken);
            var allCountriesResult = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 250 }), cancellationToken);
            ViewBag.AvailableLeagues = allLeaguesResult.Items;
            ViewBag.AvailableCountries = allCountriesResult.Items;
            return validation.ToFormResult(this, command, nameof(Index));
        }

        var result = await _commands.CreatePredictClub.HandleAsync(command, cancellationToken);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "Predict Club created successfully!";
            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        var leagues = await _queries.GetLeagues.HandleAsync(new GetLeaguesQuery(new LeagueFilter { PageSize = 200 }), cancellationToken);
        var countries = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 250 }), cancellationToken);
        ViewBag.AvailableLeagues = leagues.Items;
        ViewBag.AvailableCountries = countries.Items;

        return result.ToFormResult(this, command, nameof(Index));
    }

    // GET: /PredictClub/Join/{id}
    [HttpGet]
    [DisplayName("Join Predict Club")]
    [Description("Join a Predict Club with direct link or enter private join code.")]
    public async Task<IActionResult> Join(Guid id, [FromQuery] string? code, CancellationToken cancellationToken)
    {
        var result = await _queries.GetPredictClubDetails.HandleAsync(new GetPredictClubDetailsQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        ViewBag.Club = result.Value.Club;
        ViewBag.Code = code;

        return View(new JoinPredictClubCommand(id, code));
    }

    // POST: /PredictClub/Join
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Join Predict Club (Submit)")]
    [Description("Confirm membership into a Predict Club.")]
    public async Task<IActionResult> Join(JoinPredictClubCommand command, CancellationToken cancellationToken)
    {
        var result = await _commands.JoinPredictClub.HandleAsync(command, cancellationToken);
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = "You have joined the Predict Club!";
            return RedirectToAction(nameof(Details), new { id = command.PredictClubId });
        }

        var clubResult = await _queries.GetPredictClubDetails.HandleAsync(new GetPredictClubDetailsQuery(command.PredictClubId), cancellationToken);
        if (clubResult.IsSuccess) ViewBag.Club = clubResult.Value.Club;

        return result.ToFormResult(this, command, nameof(Index));
    }

    // POST: /PredictClub/PredictMatch
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Match Prediction")]
    [Description("Save or submit a match fact prediction draft for an upcoming fixture.")]
    public async Task<IActionResult> PredictMatch(SaveMatchPredictionCommand command, CancellationToken cancellationToken)
    {
        var result = await _commands.SaveMatchPrediction.HandleAsync(command, cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? (command.Submit ? "Prediction submitted and saved!" : "Draft prediction saved successfully.")
            : result.Error;

        return RedirectToAction(nameof(Details), new { id = command.PredictClubId });
    }

    // POST: /PredictClub/PredictTournament
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Tournament Prediction")]
    [Description("Save overall tournament predictions (1st, 2nd, 3rd, MVP, Top Scorer).")]
    public async Task<IActionResult> PredictTournament(SaveTournamentPredictionCommand command, CancellationToken cancellationToken)
    {
        var result = await _commands.SaveTournamentPrediction.HandleAsync(command, cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Tournament predictions saved successfully!"
            : result.Error;

        return RedirectToAction(nameof(Details), new { id = command.PredictClubId });
    }
}
