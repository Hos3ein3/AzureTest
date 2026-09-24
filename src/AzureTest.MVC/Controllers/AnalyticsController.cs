using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetResumeAnalytics;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class AnalyticsController : Controller
{
    private readonly IResumeQueryUnit _queries;

    public AnalyticsController(IResumeQueryUnit queries)
    {
        _queries = queries;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var data = await _queries.GetAnalytics.HandleAsync(new GetResumeAnalyticsQuery(), cancellationToken);
        return View(data);
    }
}
