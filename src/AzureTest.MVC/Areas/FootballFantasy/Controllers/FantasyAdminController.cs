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

[Area("FootballFantasy")]
[Authorize]
[DisplayName("Fantasy Admin")]
[Description("Manage real-world football data (countries, leagues, teams, players, fixtures, stats) and fantasy leagues.")]
public class FantasyAdminController : Controller
{
    private readonly IFootballFantasyQueryUnit _queries;
    private readonly ICurrentUser _currentUser;
    private readonly IFootballApiService _apiService;

    public FantasyAdminController(
        IFootballFantasyQueryUnit queries,
        ICurrentUser currentUser,
        IFootballApiService apiService)
    {
        _queries = queries;
        _currentUser = currentUser;
        _apiService = apiService;
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

        // Fetch API Sync Settings
        var apiSettings = await _apiService.GetSettingsAsync(cancellationToken);
        ViewBag.ApiSettings = apiSettings;

        return View(result.Value);
    }

    // GET: /FootballFantasy/FantasyAdmin/ApiSync
    [HttpGet]
    [DisplayName("ApiSync")]
    [Description("Manage API-Football integration settings, API credentials and trigger on-demand synchronizations.")]
    public async Task<IActionResult> ApiSync(CancellationToken cancellationToken)
    {
        var settings = await _apiService.GetSettingsAsync(cancellationToken);
        return View(settings);
    }

    // POST: /FootballFantasy/FantasyAdmin/SaveApiSettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("SaveApiSettings")]
    [Description("Save API-Football integration credentials and connection settings.")]
    public async Task<IActionResult> SaveApiSettings(AzureTest.Contracts.DTOs.FootballFantasy.FootballApiProviderSettingsDto model, CancellationToken cancellationToken)
    {
        await _apiService.SaveSettingsAsync(model, cancellationToken);
        TempData["SuccessMessage"] = "API Provider settings saved successfully.";
        return RedirectToAction(nameof(ApiSync));
    }

    // POST: /FootballFantasy/FantasyAdmin/TriggerSync
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("TriggerSync")]
    [Description("Trigger real-world football data synchronization from API-Football.")]
    public async Task<IActionResult> TriggerSync(string domain, CancellationToken cancellationToken)
    {
        AzureTest.Contracts.DTOs.FootballFantasy.ApiSyncResultDto syncResult = domain?.ToLowerInvariant() switch
        {
            "countries" => await _apiService.SyncCountriesAsync(cancellationToken),
            "leagues" => await _apiService.SyncLeaguesAsync(cancellationToken),
            "teams" => await _apiService.SyncTeamsAsync(null, cancellationToken),
            "players" => await _apiService.SyncPlayersAsync(null, cancellationToken),
            "fixtures" => await _apiService.SyncFixturesAndStatsAsync(null, cancellationToken),
            "all" => await RunAllSyncAsync(cancellationToken),
            _ => new AzureTest.Contracts.DTOs.FootballFantasy.ApiSyncResultDto { IsSuccess = false, Message = $"Unknown sync domain: '{domain}'." }
        };

        if (syncResult.IsSuccess)
        {
            TempData["SuccessMessage"] = $"Sync [{domain}] completed successfully: {syncResult.Message}";
        }
        else
        {
            TempData["ErrorMessage"] = $"Sync [{domain}] failed: {syncResult.Message}";
        }

        return RedirectToAction(nameof(ApiSync));
    }

    private async Task<AzureTest.Contracts.DTOs.FootballFantasy.ApiSyncResultDto> RunAllSyncAsync(CancellationToken cancellationToken)
    {
        var cRes = await _apiService.SyncCountriesAsync(cancellationToken);
        var lRes = await _apiService.SyncLeaguesAsync(cancellationToken);
        var tRes = await _apiService.SyncTeamsAsync(null, cancellationToken);
        var pRes = await _apiService.SyncPlayersAsync(null, cancellationToken);
        var fRes = await _apiService.SyncFixturesAndStatsAsync(null, cancellationToken);

        return new AzureTest.Contracts.DTOs.FootballFantasy.ApiSyncResultDto
        {
            IsSuccess = cRes.IsSuccess && lRes.IsSuccess && tRes.IsSuccess && pRes.IsSuccess && fRes.IsSuccess,
            Message = $"Full Sync finished. Countries: {cRes.ItemsProcessed}, Leagues: {lRes.ItemsProcessed}, Teams: {tRes.ItemsProcessed}, Players: {pRes.ItemsProcessed}, Fixtures & Stats: {fRes.ItemsProcessed}",
            ItemsProcessed = cRes.ItemsProcessed + lRes.ItemsProcessed + tRes.ItemsProcessed + pRes.ItemsProcessed + fRes.ItemsProcessed
        };
    }
}
