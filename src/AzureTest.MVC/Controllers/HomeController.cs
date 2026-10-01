using AzureTest.Application.Abstractions;


using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AzureTest.MVC.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetResumeAnalytics;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Events;
using AzureTest.Application.Expenses.Units;
using AzureTest.Application.Habits;
using AzureTest.Application.Tasks;
using AzureTest.Application.Tickets;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IResumeQueryUnit _queries;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventQueryUnit _eventQuery;
    private readonly ITaskQueryUnit _taskQuery;
    private readonly ITicketQueryUnit _ticketQuery;
    private readonly IHabitQueryUnit _habitQuery;
    private readonly IDebtQueryUnit _debtQuery;
    private readonly IExpenseQueryUnit _expenseQuery;

    public HomeController(
        IResumeQueryUnit queries, 
        UserManager<ApplicationUser> userManager,
        IEventQueryUnit eventQuery,
        ITaskQueryUnit taskQuery,
        ITicketQueryUnit ticketQuery,
        IHabitQueryUnit habitQuery,
        IDebtQueryUnit debtQuery,
        IExpenseQueryUnit expenseQuery)
    {
        _queries = queries;
        _userManager = userManager;
        _eventQuery = eventQuery;
        _taskQuery = taskQuery;
        _ticketQuery = ticketQuery;
        _habitQuery = habitQuery;
        _debtQuery = debtQuery;
        _expenseQuery = expenseQuery;
    }

    public async Task<IActionResult> Index(
        string? userId, 
        [FromServices] AzureTest.Application.Messaging.IQueryHandler<AzureTest.Application.UserPreferences.Queries.GetUserPreferenceQuery, AzureTest.Domain.Documents.UserPreference> prefQuery,
        [FromQuery] bool ignorePreference,
        CancellationToken cancellationToken)
    {
        var currentUserIdStr = _userManager.GetUserId(User);
        var isAdmin = User.IsInRole("Admin");
        
        if (currentUserIdStr != null && !ignorePreference && string.IsNullOrWhiteSpace(userId))
        {
            var pref = await prefQuery.HandleAsync(new AzureTest.Application.UserPreferences.Queries.GetUserPreferenceQuery(Guid.Parse(currentUserIdStr)), cancellationToken);
            if (pref != null && !string.IsNullOrWhiteSpace(pref.PreferredDashboard))
            {
                return Redirect($"/{pref.PreferredDashboard}");
            }
        }
        
        var targetUserIdStr = userId;
        if (string.IsNullOrWhiteSpace(userId) && !isAdmin)
        {
            targetUserIdStr = currentUserIdStr;
        }

        var data = await _queries.GetAnalytics.HandleAsync(new GetResumeAnalyticsQuery(targetUserIdStr), cancellationToken);
        
        var model = new HomeDashboardViewModel
        {
            Analytics = data,
            IsAdmin = data.IsAdmin,
            CurrentUserId = targetUserIdStr ?? string.Empty,
            IsViewingOtherUser = !string.IsNullOrWhiteSpace(targetUserIdStr) && targetUserIdStr != currentUserIdStr
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

            // Tickets
            if (model.IsAdmin && !parsedUserId.HasValue)
            {
                // Admin viewing aggregate -> show unanswered
                model.Tickets = await _ticketQuery.GetUnansweredTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetUnansweredTicketsQuery(), cancellationToken);
            }
            else
            {
                // User or Admin viewing specific user -> show their unread/open
                model.Tickets = await _ticketQuery.GetUnreadTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetUnreadTicketsQuery(parsedUserId ?? Guid.Empty), cancellationToken);
            }
            
            // Ticket Analytics
            var allUserTickets = await _ticketQuery.GetAllTickets.HandleAsync(new AzureTest.Application.Tickets.Queries.GetAllTicketsQuery(parsedUserId), cancellationToken);
            model.TicketStatusCounts = allUserTickets.GroupBy(t => t.Status.ToString()).ToDictionary(g => g.Key, g => g.Count());
            
            // Habit Adherence (Last 30 days)
            int doneHabits = 0;
            int pendingHabits = 0;
            var thirtyDaysAgo = now.AddDays(-30);
            foreach (var h in allHabits)
            {
                var recentLogs = h.Logs.Where(l => l.LogDate >= thirtyDaysAgo).ToList();
                foreach (var log in recentLogs)
                {
                    bool isDone = h.Type == AzureTest.Domain.Enums.HabitType.Boolean ? log.BooleanValue : log.NumericValue >= h.Goal;
                    if (isDone) doneHabits++;
                    else pendingHabits++;
                }
            }
            model.HabitAdherence["Done"] = doneHabits;
            model.HabitAdherence["Pending/Failed"] = pendingHabits;

            // Recent Debts
            var allDebts = await _debtQuery.GetAll.HandleAsync(new AzureTest.Application.Debts.Queries.GetAllDebtsQuery(parsedUserId), cancellationToken);
            model.RecentDebts = allDebts.Take(5).ToList();
            
            // Promissed Debts (Due Soon)
            model.PromissedDebts = allDebts
                .Where(d => d.PromissedReturnDate.HasValue && !d.IsDone)
                .OrderBy(d => d.PromissedReturnDate)
                .Take(5).ToList();
            
            // Debt Graph Data
            model.DebtsByPerson = allDebts
                .Where(d => d.Amount > 0)
                .GroupBy(d => d.DebtPersonName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Amount));
            
            // Recent Expenses
            var allExpenses = await _expenseQuery.GetAll.HandleAsync(new AzureTest.Application.Expenses.Queries.GetAllExpensesQuery(
                new AzureTest.Contracts.DTOs.Expenses.GetAllExpensesFilters 
                { 
                    UserId = parsedUserId,
                    PageSize = 1000,
                    SortBy = "Date",
                    SortDescending = true
                }), cancellationToken);
            
            model.RecentExpenses = allExpenses.Items.Take(5).ToList();
            
            // Expense Graph Data
            model.ExpensesByCategory = allExpenses.Items
                .Where(e => e.Amount > 0)
                .GroupBy(e => string.IsNullOrWhiteSpace(e.CategoryName) ? "Uncategorized" : e.CategoryName)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
                
            // Monthly Trend (Last 6 Months)
            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthKey = monthDate.ToString("MMM yyyy");
                
                var monthExpenses = allExpenses.Items.Where(e => e.Date.Year == monthDate.Year && e.Date.Month == monthDate.Month).Sum(e => e.Amount);
                var monthDebts = allDebts.Where(d => d.DateOfBorrow.Year == monthDate.Year && d.DateOfBorrow.Month == monthDate.Month).Sum(d => d.Amount);
                
                model.MonthlyExpenses[monthKey] = monthExpenses;
                model.MonthlyDebts[monthKey] = monthDebts;
            }
                
            // Analytics KPIs
            model.TotalOutstandingDebt = allDebts.Where(d => !d.IsDone).Sum(d => d.Amount);
            model.TotalExpensesThisMonth = model.MonthlyExpenses.GetValueOrDefault(now.ToString("MMM yyyy"), 0);
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