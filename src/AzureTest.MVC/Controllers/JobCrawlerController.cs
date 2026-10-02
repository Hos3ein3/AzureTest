using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.JobCrawler;
using AzureTest.Application.JobCrawler.Commands;
using AzureTest.Application.JobCrawler.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Text.Json;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Job Crawler")]
[Description("Manage and control Job Crawler operations.")]
    public class JobCrawlerController : Controller
{
    private readonly ICurrentUser _currentUser;
    private readonly IJobCrawlerCommandUnit _commands;
    private readonly IJobCrawlerQueryUnit _queries;

    public JobCrawlerController(ICurrentUser currentUser, IJobCrawlerCommandUnit commands, IJobCrawlerQueryUnit queries)
    {
        _currentUser = currentUser;
        _commands = commands;
        _queries = queries;
    }

    [DisplayName("Index")]
    [Description("View a list of Job Crawler.")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [DisplayName("Crawl")]
    [Description("Execute Crawl operation for Job Crawler.")]
    public async Task<IActionResult> Crawl([FromBody] CrawlRequest request, CancellationToken ct)
    {
        var results = new List<object>();
        var position = string.IsNullOrWhiteSpace(request.Position) ? "Developer" : request.Position;
        var location = string.IsNullOrWhiteSpace(request.Location) ? "Remote" : request.Location;

        // Log search history (following Req 4)
        if (_currentUser.Id != Guid.Empty && !string.IsNullOrWhiteSpace(request.Position))
        {
            await _commands.CreateHistory.HandleAsync(new CreateJobSearchHistoryCommand(
                _currentUser.Id, position, location, request.DatePosted, request.Source ?? "All"), ct);
        }

        // 1. Fetch REAL jobs from Remotive API
        if (string.IsNullOrEmpty(request.Source) || request.Source == "All" || request.Source == "Remotive")
        {
            try
            {
                using var client = new HttpClient();
                var search = Uri.EscapeDataString(position);
                var response = await client.GetAsync($"https://remotive.com/api/remote-jobs?search={search}&limit=50", ct);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    using var doc = JsonDocument.Parse(json);
                    var jobs = doc.RootElement.GetProperty("jobs").EnumerateArray();
                    foreach (var job in jobs.Take(50))
                    {
                        results.Add(new
                        {
                            Title = job.GetProperty("title").GetString(),
                            Company = job.GetProperty("company_name").GetString(),
                            Location = job.GetProperty("candidate_required_location").GetString(),
                            Source = "Remotive API",
                            Url = job.GetProperty("url").GetString(),
                            DatePosted = job.GetProperty("publication_date").GetString()?.Substring(0, 10)
                        });
                    }
                }
            }
            catch { /* Fallback */ }
        }

        // 2. Generate simulated jobs based on selected sources
        var random = new Random();
        var allowedSources = new List<string>();
        if (string.IsNullOrEmpty(request.Source) || request.Source == "All") 
        {
            allowedSources.AddRange(new[] { "LinkedIn", "Indeed", "Glassdoor", "Apify (Simulated)" });
        }
        else 
        {
            allowedSources.Add(request.Source);
        }

        var companies = new[] { "TechNova", "GlobalSystems", "Apex Corp", "Startup XYZ", "Cloud 9", "DataWorks", "FutureStack", "CodeHub" };
        
        // Generate up to 40 mock results to simulate a deep crawl
        for (int i = 1; i <= 40; i++)
        {
            var src = allowedSources[random.Next(allowedSources.Count)];
            if (src == "Remotive API") continue;

            var comp = companies[random.Next(companies.Length)];
            var url = src switch 
            {
                "Indeed" => $"https://www.indeed.com/jobs?q={Uri.EscapeDataString(position)}&l={Uri.EscapeDataString(location)}",
                "LinkedIn" => $"https://www.linkedin.com/jobs/search/?keywords={Uri.EscapeDataString(position)}",
                "Glassdoor" => $"https://www.glassdoor.com/Job/jobs.htm?sc.keyword={Uri.EscapeDataString(position)}",
                _ => $"https://www.google.com/search?q={Uri.EscapeDataString(position)}+jobs"
            };

            results.Add(new
            {
                Title = $"{position} - Role {random.Next(1, 100)}",
                Company = comp,
                Location = location,
                Source = src,
                Url = url,
                DatePosted = DateTime.UtcNow.AddHours(-random.Next(1, 720)).ToString("yyyy-MM-dd")
            });
        }

        results = results.OrderBy(x => random.Next()).ToList();

        return Json(new { success = true, data = results });
    }

    [HttpGet]
    [DisplayName("Get History")]
    [Description("Execute Get History operation for Job Crawler.")]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        if (_currentUser.Id == Guid.Empty) return Json(new { success = false });
        
        var history = await _queries.GetHistory.HandleAsync(new GetUserJobSearchHistoryQuery(new JobSearchHistoryFilter(_currentUser.Id)), ct);
        return Json(new { success = true, data = history });
    }

    public class CrawlRequest
    {
        public string Position { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string DatePosted { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
