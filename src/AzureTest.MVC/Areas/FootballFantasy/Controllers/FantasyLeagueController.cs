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
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Area("FootballFantasy")]
[Authorize]
[DisplayName("Fantasy League")]
[Description("Manage and participate in Football Fantasy leagues, teams and squads.")]
public class FantasyLeagueController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly IFootballFantasyCommandUnit _commands;
    private readonly ICurrentUser _currentUser;
    private readonly IValidationService _validator;

    public FantasyLeagueController(
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

    // GET: /FantasyLeague
    [HttpGet]
    [DisplayName("Index")]
    [Description("View a list of fantasy leagues with filtering, searching and pagination.")]
    public async Task<IActionResult> Index(FantasyLeagueFilter filter, CancellationToken cancellationToken)
    {
        var result = await _queries.GetFantasyLeagues.HandleAsync(new GetFantasyLeaguesQuery(filter), cancellationToken);
        ViewBag.Filter = filter;
        ViewBag.IsAdmin = _currentUser.IsAdmin;
        return View(result);
    }

    // GET: /FantasyLeague/Details/{id}
    [HttpGet]
    [DisplayName("Details")]
    [Description("View details, standings, and rules of a fantasy league.")]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queries.GetFantasyLeagueById.HandleAsync(new GetFantasyLeagueByIdQuery(id), cancellationToken);
        if (result.IsFailure) return NotFound();

        // Check if user already has a team
        var userTeamResult = await _queries.GetFantasyTeamByLeague.HandleAsync(new GetFantasyTeamByLeagueQuery(id, _currentUser.Id), cancellationToken);
        ViewBag.UserTeam = userTeamResult.IsSuccess ? userTeamResult.Value : null;

        // Build invite link for private leagues
        var request = HttpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var inviteLink = result.Value.Visibility == FantasyLeagueVisibility.Private && !string.IsNullOrWhiteSpace(result.Value.JoinCode)
            ? $"{baseUrl}/FantasyLeague/Join/{result.Value.Id}?code={Uri.EscapeDataString(result.Value.JoinCode)}"
            : null;
        ViewBag.InviteLink = inviteLink;

        // Fetch tournament details (H2H, Knockout brackets, Groups) if applicable
        var tournamentResult = await _queries.GetLeagueTournamentDetails.HandleAsync(new GetLeagueTournamentDetailsQuery(id), cancellationToken);
        ViewBag.TournamentGroups = tournamentResult.IsSuccess ? tournamentResult.Value : new List<FantasyLeagueGroupDto>();

        // Owner join requests count
        int pendingRequestsCount = 0;
        if (result.Value.OwnerUserId == _currentUser.Id || _currentUser.IsAdmin)
        {
            var reqResult = await _queries.GetPendingJoinRequests.HandleAsync(new GetPendingJoinRequestsQuery(id), cancellationToken);
            if (reqResult.IsSuccess)
            {
                pendingRequestsCount = reqResult.Value.Count;
            }
        }
        ViewBag.PendingRequestsCount = pendingRequestsCount;

        // Check if current user has pending approval request
        bool isPendingApproval = false;
        if (userTeamResult.IsSuccess && !userTeamResult.Value.IsApproved)
        {
            isPendingApproval = true;
        }
        ViewBag.IsPendingApproval = isPendingApproval;

        return View(result.Value);
    }

    // GET: /FantasyLeague/Create
    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new fantasy football league.")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.IsAdmin = _currentUser.IsAdmin;
        var adminDash = await _queries.GetAdminDashboard.HandleAsync(new GetFantasyAdminDashboardQuery(), cancellationToken);
        var allLeaguesResult = await _queries.GetLeagues.HandleAsync(new GetLeaguesQuery(new LeagueFilter { PageSize = 200 }), cancellationToken);
        var allCountriesResult = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 250 }), cancellationToken);

        ViewBag.AvailableLeagues = allLeaguesResult.Items;
        ViewBag.AvailableCountries = allCountriesResult.Items;
        if (adminDash.IsSuccess)
        {
            ViewBag.Countries = adminDash.Value.RecentCountries;
            ViewBag.Leagues = adminDash.Value.RecentLeagues;
        }

        var model = new CreateFantasyLeagueCommand(
            Name: string.Empty,
            Description: null,
            Visibility: FantasyLeagueVisibility.Public,
            PeriodType: FantasyPeriodType.Weekly,
            SquadMode: FantasySquadMode.SeasonSquad,
            SquadSize: FantasyLeague.Rules.DefaultSquadSize,
            StartingSize: FantasyLeague.Rules.DefaultStartingSize,
            Budget: FantasyLeague.Rules.DefaultBudget,
            AllowedFormations: FantasyLeague.Rules.DefaultAllowedFormations,
            JoinCode: null,
            StartsAt: DateTime.UtcNow.Date.AddDays(1),
            EndsAt: DateTime.UtcNow.Date.AddMonths(9),
            MaxMembers: FantasyLeague.Rules.MaxLeagueMembers,
            CustomTag: null,
            ScopeType: PoolScopeType.All,
            ScopeValue: null,
            AllowCoach: true
        );
        return View(model);
    }

    // POST: /FantasyLeague/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new fantasy football league.")]
    public async Task<IActionResult> Create(CreateFantasyLeagueCommand model, CancellationToken cancellationToken)
    {
        ViewBag.IsAdmin = _currentUser.IsAdmin;
        var adminDash = await _queries.GetAdminDashboard.HandleAsync(new GetFantasyAdminDashboardQuery(), cancellationToken);
        var allLeaguesResult = await _queries.GetLeagues.HandleAsync(new GetLeaguesQuery(new LeagueFilter { PageSize = 200 }), cancellationToken);
        var allCountriesResult = await _queries.GetCountries.HandleAsync(new GetCountriesQuery(new CountryFilter { PageSize = 250 }), cancellationToken);
        ViewBag.AvailableLeagues = allLeaguesResult.Items;
        ViewBag.AvailableCountries = allCountriesResult.Items;
        if (adminDash.IsSuccess)
        {
            ViewBag.Countries = adminDash.Value.RecentCountries;
            ViewBag.Leagues = adminDash.Value.RecentLeagues;
        }

        // Security check: only admin can create Global leagues
        if (model.Visibility == FantasyLeagueVisibility.Global && !_currentUser.IsAdmin)
        {
            ModelState.AddModelError(nameof(model.Visibility), "Only system administrators can create Global fantasy leagues.");
            return View(model);
        }

        var validation = await _validator.ValidateAsync(model, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, model, nameof(Index));
        }

        var result = await _commands.CreateFantasyLeague.HandleAsync(model, cancellationToken);
        return result.ToFormResult(this, model, nameof(Index));
    }

    // GET: /FantasyLeague/Join/{id}
    [HttpGet]
    [DisplayName("Join League")]
    [Description("Create a fantasy team and join a fantasy league.")]
    public async Task<IActionResult> Join(Guid id, [FromQuery] string? code, CancellationToken cancellationToken)
    {
        var leagueResult = await _queries.GetFantasyLeagueById.HandleAsync(new GetFantasyLeagueByIdQuery(id), cancellationToken);
        if (leagueResult.IsFailure) return NotFound();

        var league = leagueResult.Value;

        // Check if league is full
        if (league.MaxMembers.HasValue && league.MemberCount >= league.MaxMembers.Value)
        {
            TempData["ErrorMessage"] = $"This fantasy league has reached its maximum capacity of {league.MaxMembers.Value} teams.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var teamResult = await _queries.GetFantasyTeamByLeague.HandleAsync(new GetFantasyTeamByLeagueQuery(id, _currentUser.Id), cancellationToken);
        if (teamResult.IsSuccess)
        {
            return RedirectToAction(nameof(Team), new { id = teamResult.Value.Id });
        }

        if (league.AllowCoach)
        {
            var coachesResult = await _queries.GetCoaches.HandleAsync(new GetCoachesQuery(new CoachFilter { PageSize = 100 }), cancellationToken);
            ViewBag.Coaches = coachesResult.Items;
        }

        ViewBag.League = league;
        var model = new JoinLeagueRequest(id, string.Empty, code ?? string.Empty, null, null);
        return View(model);
    }

    // POST: /FantasyLeague/Join
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Join League")]
    [Description("Submit fantasy team creation and join the fantasy league.")]
    public async Task<IActionResult> Join(JoinLeagueRequest request, CancellationToken cancellationToken)
    {
        var leagueResult = await _queries.GetFantasyLeagueById.HandleAsync(new GetFantasyLeagueByIdQuery(request.FantasyLeagueId), cancellationToken);
        if (leagueResult.IsFailure) return NotFound();

        var league = leagueResult.Value;

        // Enforce max player / team count rule
        if (league.MaxMembers.HasValue && league.MemberCount >= league.MaxMembers.Value)
        {
            ModelState.AddModelError(string.Empty, $"Cannot join: This fantasy league has reached the maximum allowed limit of {league.MaxMembers.Value} teams.");
            ViewBag.League = league;
            return View(request);
        }

        var cmd = new CreateFantasyTeamCommand(request.FantasyLeagueId, request.TeamName, request.LogoUrl, request.JoinCode, request.CoachId);
        var result = await _commands.CreateFantasyTeam.HandleAsync(cmd, cancellationToken);

        if (result.IsSuccess)
        {
            return RedirectToAction(nameof(Team), new { id = result.Value });
        }

        if (league.AllowCoach)
        {
            var coachesResult = await _queries.GetCoaches.HandleAsync(new GetCoachesQuery(new CoachFilter { PageSize = 100 }), cancellationToken);
            ViewBag.Coaches = coachesResult.Items;
        }

        ViewBag.League = league;
        return result.ToFormResult(this, request, nameof(Team), "FantasyLeague");
    }

    // GET: /FantasyLeague/Team/{id}
    [HttpGet]
    [DisplayName("Team Pitch & Lineup")]
    [Description("Interactive FIFA Ultimate Team cards pitch visualization and lineup manager.")]
    public async Task<IActionResult> Team(Guid id, PlayerFilter? playerFilter, CancellationToken cancellationToken)
    {
        var teamResult = await _queries.GetFantasyTeamById.HandleAsync(new GetFantasyTeamByIdQuery(id), cancellationToken);
        if (teamResult.IsFailure) return NotFound();

        var leagueResult = await _queries.GetFantasyLeagueById.HandleAsync(new GetFantasyLeagueByIdQuery(teamResult.Value.FantasyLeagueId), cancellationToken);

        playerFilter ??= new PlayerFilter { PageSize = 40 };
        playerFilter.FantasyLeagueId = teamResult.Value.FantasyLeagueId;
        var playersResult = await _queries.GetPlayers.HandleAsync(new GetPlayersQuery(playerFilter), cancellationToken);

        if (leagueResult.IsSuccess && leagueResult.Value.AllowCoach)
        {
            var coachesResult = await _queries.GetCoaches.HandleAsync(new GetCoachesQuery(new CoachFilter { PageSize = 100 }), cancellationToken);
            ViewBag.Coaches = coachesResult.Items;
        }

        ViewBag.Team = teamResult.Value;
        ViewBag.League = leagueResult.IsSuccess ? leagueResult.Value : null;
        ViewBag.Players = playersResult.Items;
        ViewBag.PlayerFilter = playerFilter;

        return View(teamResult.Value);
    }

    // POST: /FantasyLeague/SaveLineup
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Save Lineup")]
    [Description("Save the pitch tactical formation and selected lineup slots.")]
    public async Task<IActionResult> SaveLineup([FromBody] SaveLineupRequest request, CancellationToken cancellationToken)
    {
        var result = await _commands.SaveLineup.HandleAsync(new SaveLineupCommand(request), cancellationToken);
        return Json(new { success = result.IsSuccess, message = result.Message ?? result.Error });
    }

    // GET: /FantasyLeague/JoinRequests/{id}
    [HttpGet]
    [DisplayName("Join Requests")]
    [Description("View and manage pending manager requests to join this fantasy league.")]
    public async Task<IActionResult> JoinRequests(Guid id, CancellationToken cancellationToken)
    {
        var leagueResult = await _queries.GetFantasyLeagueById.HandleAsync(new GetFantasyLeagueByIdQuery(id), cancellationToken);
        if (leagueResult.IsFailure) return NotFound();

        var requestsResult = await _queries.GetPendingJoinRequests.HandleAsync(new GetPendingJoinRequestsQuery(id), cancellationToken);
        if (requestsResult.IsFailure)
        {
            TempData["ErrorMessage"] = requestsResult.Error;
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewBag.League = leagueResult.Value;
        return View(requestsResult.Value);
    }

    // POST: /FantasyLeague/ReviewJoin
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Review Join Request")]
    [Description("Approve or reject a manager request to join a public fantasy league.")]
    public async Task<IActionResult> ReviewJoin(Guid leagueId, Guid targetUserId, bool approve, [FromQuery] string? returnUrl, CancellationToken cancellationToken)
    {
        var result = await _commands.ReviewJoinRequest.HandleAsync(new ReviewJoinRequestCommand(leagueId, targetUserId, approve), cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess 
            ? (approve ? "Manager join request approved." : "Manager join request rejected.") 
            : result.Error;

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(JoinRequests), new { id = leagueId });
    }

    // POST: /FantasyLeague/WithdrawJoin
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Withdraw Join Request")]
    [Description("Withdraw and cancel your pending application to join a fantasy league.")]
    public async Task<IActionResult> WithdrawJoin(Guid leagueId, CancellationToken cancellationToken)
    {
        var result = await _commands.WithdrawJoinRequest.HandleAsync(new WithdrawJoinRequestCommand(leagueId), cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Your join request has been successfully withdrawn."
            : result.Error;
        return RedirectToAction(nameof(Details), new { id = leagueId });
    }

    // POST: /FantasyLeague/ProcessKickouts
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Process Unfinalized Kickouts")]
    [Description("Enforce unfinalized squad rules and kick out or demote incomplete teams.")]
    public async Task<IActionResult> ProcessKickouts(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.ProcessLeagueKickouts.HandleAsync(new ProcessLeagueKickoutsCommand(id), cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? $"Processed kickouts for {result.Value} unfinalized teams."
            : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: /FantasyLeague/Auction/{id}
    [HttpGet]
    [DisplayName("Live Auction Room")]
    [Description("Real-time SignalR player auction bidding room for exclusive player ownership.")]
    public async Task<IActionResult> Auction(Guid id, CancellationToken cancellationToken)
    {
        var leagueResult = await _queries.GetFantasyLeagueById.HandleAsync(new GetFantasyLeagueByIdQuery(id), cancellationToken);
        if (leagueResult.IsFailure) return NotFound();

        var teamResult = await _queries.GetFantasyTeamByLeague.HandleAsync(new GetFantasyTeamByLeagueQuery(id, _currentUser.Id), cancellationToken);
        if (teamResult.IsFailure)
        {
            TempData["ErrorMessage"] = "You must join this league with a team before entering the auction room.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var playersResult = await _queries.GetPlayers.HandleAsync(new GetPlayersQuery(new PlayerFilter { FantasyLeagueId = id, PageSize = 100 }), cancellationToken);

        ViewBag.League = leagueResult.Value;
        ViewBag.Team = teamResult.Value;
        ViewBag.Players = playersResult.Items;

        return View(leagueResult.Value);
    }

    // GET: /FantasyLeague/Profile/{userId}
    [HttpGet]
    [DisplayName("Manager Profile")]
    [Description("View a manager's fantasy football career, trophies, and achievements.")]
    public async Task<IActionResult> Profile(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _queries.GetManagerProfile.HandleAsync(new GetManagerProfileQuery(userId), cancellationToken);
        if (result.IsFailure) return NotFound();

        return View(result.Value);
    }

    // POST: /FantasyLeague/InitializeTournament
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Initialize Tournament")]
    [Description("Initialize Head-to-Head schedules, Knockout brackets, or Group stages for the league.")]
    public async Task<IActionResult> InitializeTournament(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.InitializeLeagueTournament.HandleAsync(new InitializeLeagueTournamentCommand(id), cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Tournament schedule and matchups initialized successfully!"
            : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: /FantasyLeague/Leave
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Leave League")]
    [Description("Leave a fantasy league and permanently record exit in manager history.")]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        var result = await _commands.LeaveFantasyLeague.HandleAsync(new LeaveFantasyLeagueCommand(id), cancellationToken);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "You have left the fantasy league. Your departure is recorded in your career history."
            : result.Error;
        return RedirectToAction(nameof(Details), new { id });
    }
}
