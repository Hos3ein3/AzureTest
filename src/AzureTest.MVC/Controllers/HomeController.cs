using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureTest.MVC.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetResumeAnalytics;
using AzureTest.Domain.Entities.Identity;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IResumeQueryUnit _queries;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(IResumeQueryUnit queries, UserManager<ApplicationUser> userManager)
    {
        _queries = queries;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? userId, CancellationToken cancellationToken)
    {
        var data = await _queries.GetAnalytics.HandleAsync(new GetResumeAnalyticsQuery(userId), cancellationToken);
        
        var model = new HomeDashboardViewModel
        {
            Analytics = data,
            IsAdmin = data.IsAdmin,
            CurrentUserId = userId ?? string.Empty,
        };

        if (data.IsAdmin)
        {
            var users = _userManager.Users.ToList();
            foreach (var u in users)
            {
                var displayName = $"{u.FirstName} {u.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(displayName)) displayName = u.Email ?? u.Id.ToString();
                model.AvailableUsers[u.Id.ToString()] = displayName;

                if (userId == u.Id.ToString())
                {
                    model.CurrentUserName = displayName;
                }
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                model.CurrentUserName = "All Users (Aggregate)";
            }
        }

        return View(model);
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