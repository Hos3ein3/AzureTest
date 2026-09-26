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
    private readonly AzureTest.Application.Events.IEventQueryUnit _eventQuery;
    private readonly AzureTest.Application.Tasks.ITaskQueryUnit _taskQuery;
    private readonly AzureTest.Application.Tickets.ITicketQueryUnit _ticketQuery;
    private readonly AzureTest.Application.Habits.IHabitQueryUnit _habitQuery;

    public HomeController(
        IResumeQueryUnit queries, 
        UserManager<ApplicationUser> userManager,
        AzureTest.Application.Events.IEventQueryUnit eventQuery,
        AzureTest.Application.Tasks.ITaskQueryUnit taskQuery,
        AzureTest.Application.Tickets.ITicketQueryUnit ticketQuery,
        AzureTest.Application.Habits.IHabitQueryUnit habitQuery)
    {
        _queries = queries;
        _userManager = userManager;
        _eventQuery = eventQuery;
        _taskQuery = taskQuery;
        _ticketQuery = ticketQuery;
        _habitQuery = habitQuery;
    }

    public async Task<IActionResult> Index(string? userId, CancellationToken cancellationToken)
    {
        var currentUserIdStr = _userManager.GetUserId(User);
        var targetUserIdStr = userId ?? currentUserIdStr;

        var data = await _queries.GetAnalytics.HandleAsync(new GetResumeAnalyticsQuery(targetUserIdStr), cancellationToken);
        
        var model = new HomeDashboardViewModel
        {
            Analytics = data,
            IsAdmin = data.IsAdmin,
            CurrentUserId = targetUserIdStr ?? string.Empty,
            IsViewingOtherUser = targetUserIdStr != currentUserIdStr
        };

        Guid? parsedUserId = null;
        if (!string.IsNullOrEmpty(model.CurrentUserId) && Guid.TryParse(model.CurrentUserId, out var tempId))
        {
            parsedUserId = tempId;
        }

        if (parsedUserId.HasValue || model.IsAdmin)
        {
            var now = DateTime.UtcNow;
            
            // Recent Events (upcoming within 7 days)
            var allEvents = await _eventQuery.GetAllEvents.HandleAsync(new AzureTest.Application.Events.Queries.GetAllEventsQuery(parsedUserId), cancellationToken);
            model.RecentEvents = allEvents.Where(e => e.StartDate >= now.AddDays(-1) && e.StartDate <= now.AddDays(7)).Take(5).ToList();

            // Pending Tasks
            var allTasks = await _taskQuery.GetAllTasks.HandleAsync(new AzureTest.Application.Tasks.Queries.GetAllTasksQuery(parsedUserId), cancellationToken);
            model.PendingTasks = allTasks.Where(t => !t.IsCompleted).Take(5).ToList();

            // Recent Habits
            var allHabits = await _habitQuery.GetAllHabits.HandleAsync(new AzureTest.Application.Habits.Queries.GetAllHabitsQuery(parsedUserId), cancellationToken);
            model.RecentHabits = allHabits.Take(5).ToList();

            // Tickets (Admin: Unanswered, User: Unread/Open)
            if (model.IsAdmin)
            {
                model.Tickets = await _ticketQuery.GetUnansweredTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetUnansweredTicketsQuery(), cancellationToken);
            }
            else
            {
                model.Tickets = await _ticketQuery.GetUnreadTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetUnreadTicketsQuery(parsedUserId ?? Guid.Empty), cancellationToken);
            }
        }

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