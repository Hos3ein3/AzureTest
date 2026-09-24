using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureTest.MVC.Models;

using Microsoft.AspNetCore.Authorization;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetResumeAnalytics;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IResumeQueryUnit _queries;

    public HomeController(IResumeQueryUnit queries)
    {
        _queries = queries;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var data = await _queries.GetAnalytics.HandleAsync(new GetResumeAnalyticsQuery(), cancellationToken);
        return View(data);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}