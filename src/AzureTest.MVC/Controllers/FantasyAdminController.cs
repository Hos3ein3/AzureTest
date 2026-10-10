using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.FootballFantasy;
using AzureTest.Application.FootballFantasy.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.MVC.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Fantasy Admin")]
[Description("Manage real-world football data (countries, leagues, teams, players, fixtures, stats) and fantasy leagues.")]
public class FantasyAdminController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly ICurrentUser _currentUser;

    public FantasyAdminController(IFootballFantasyQueryUnit queries, ICurrentUser currentUser)
    {
        _queries = queries;
        _currentUser = currentUser;
    }

    // GET: /FantasyAdmin
    [HttpGet]
    [DisplayName("Index")]
    [Description("Admin dashboard overview of real-world and fantasy football data.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await _queries.GetAdminDashboard.HandleAsync(new GetFantasyAdminDashboardQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToViewResult(this);
        }

        // Fetch fantasy teams overview for admin oversight
        var teamsOverview = await _queries.GetAdminFantasyTeams.HandleAsync(new GetAdminFantasyTeamsQuery(), cancellationToken);
        ViewBag.FantasyTeams = teamsOverview.IsSuccess ? teamsOverview.Value : null;

        return View(result.Value);
    }
}
