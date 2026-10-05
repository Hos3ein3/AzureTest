using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.HouseCrawler.Interfaces;
using AzureTest.Application.HouseCrawler.Commands;
using AzureTest.Application.HouseCrawler.Queries;
using AzureTest.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("House Crawler")]
[Description("Manage and control House Crawler operations.")]
public class HouseCrawlerController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly IHouseCrawlerCommandUnit _commands;
    private readonly IHouseCrawlerQueryUnit _queries;
    private readonly IHouseCrawlerService _crawlerService;

    public HouseCrawlerController(ICurrentUser currentUser, IHouseCrawlerCommandUnit commands, IHouseCrawlerQueryUnit queries, IHouseCrawlerService crawlerService)
    {
        _currentUser = currentUser;
        _commands = commands;
        _queries = queries;
        _crawlerService = crawlerService;
    }

    [DisplayName("Index")]
    [Description("View a list of House Crawler.")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [DisplayName("Crawl")]
    [Description("Execute Crawl operation for House Crawler.")]
    public async Task<IActionResult> Crawl([FromBody] HouseCrawlRequest request, CancellationToken ct)
    {
        var results = new List<HouseCrawlResult>();

        // Save History
        if (_currentUser.Id != Guid.Empty && !string.IsNullOrWhiteSpace(request.City))
        {
            await _commands.CreateHistory.HandleAsync(new CreateHouseSearchHistoryCommand(
                _currentUser.Id, request.City, request.MinPrice ?? "", request.MaxPrice ?? "", request.Bedrooms ?? "", request.Source?.ToString() ?? "All", false), ct);
        }

        // Get User Credentials
        var userCreds = _currentUser.Id != Guid.Empty 
            ? await _queries.GetUserCredentials.HandleAsync(new GetUserHouseCredentialsQuery(_currentUser.Id), ct)
            : new List<Domain.Documents.HouseCrawlerCredential>();

        var targets = new List<HouseWebsite>();
        if (request.Source.HasValue && request.Source != 0) // Assume 0 means All for this DTO if it was enum, but it's nullable enum. Actually we'll use enum directly.
        {
            targets.Add(request.Source.Value);
        }
        else
        {
            targets.AddRange(Enum.GetValues<HouseWebsite>());
        }

        // Crawl all targets in parallel
        var tasks = targets.Select(target => 
        {
            var cred = userCreds.FirstOrDefault(c => c.Website == target);
            return _crawlerService.CrawlAsync(request.City ?? "Amsterdam", request.MinPrice ?? "", request.MaxPrice ?? "", request.Bedrooms ?? "", target, cred, ct);
        });

        var crawlResults = await Task.WhenAll(tasks);
        foreach(var resList in crawlResults)
        {
            results.AddRange(resList);
        }

        return Json(new { success = true, count = results.Count, data = results });
    }

    [HttpGet]
    [DisplayName("GetHistory")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var history = await _queries.GetUserHistory.HandleAsync(new GetUserHouseSearchHistoryQuery(_currentUser.Id), ct);
        return Json(new { success = true, data = history });
    }

    [HttpPost]
    [DisplayName("SaveCredential")]
    public async Task<IActionResult> SaveCredential([FromBody] HouseCredentialRequest request, CancellationToken ct)
    {
        if (_currentUser.Id == Guid.Empty) return Unauthorized();

        await _commands.SaveCredential.HandleAsync(new SaveHouseCredentialCommand(
            _currentUser.Id, request.Website, request.Username, request.Password), ct);

        return Json(new { success = true });
    }

    [HttpGet]
    [DisplayName("GetCredentials")]
    public async Task<IActionResult> GetCredentials(CancellationToken ct)
    {
        var creds = await _queries.GetUserCredentials.HandleAsync(new GetUserHouseCredentialsQuery(_currentUser.Id), ct);
        return Json(new { success = true, data = creds.Select(c => new { website = c.Website.ToString(), websiteValue = (int)c.Website, username = c.Username }) });
    }
}

public class HouseCrawlRequest
{
    public string City { get; set; } = string.Empty;
    public string? MinPrice { get; set; }
    public string? MaxPrice { get; set; }
    public string? Bedrooms { get; set; }
    public HouseWebsite? Source { get; set; }
}

public class HouseCredentialRequest
{
    public HouseWebsite Website { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
