using AzureTest.Application.Abstractions;


using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetResumeAnalytics;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Analytics")]
[Description("Manage and control Analytics operations.")]
    public class AnalyticsController : Controller
{
    private readonly IResumeQueryUnit _queries;

    public AnalyticsController(IResumeQueryUnit queries)
    {
        _queries = queries;
    }

    [DisplayName("Index")]
    [Description("View a list of Analytics.")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var data = await _queries.GetAnalytics.HandleAsync(new GetResumeAnalyticsQuery(), cancellationToken);
        return View(data);
    }
}
