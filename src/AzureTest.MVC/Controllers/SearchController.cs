using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly IUnitOfWork _sqlUow;
    private readonly INoSqlUnitOfWork _noSqlUow;
    private readonly ICurrentUser _currentUser;

    public SearchController(IUnitOfWork sqlUow, INoSqlUnitOfWork noSqlUow, ICurrentUser currentUser)
    {
        _sqlUow = sqlUow;
        _noSqlUow = noSqlUow;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Omnibar(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return Json(new List<object>());
        
        q = q.ToLowerInvariant();
        var results = new List<object>();
        var userId = _currentUser.Id;

        // 1. Search Tasks
        var tasks = await _sqlUow.Tasks.GetTasksForUserAsync(userId);
        var filteredTasks = tasks.Where(t => t.Title.ToLower().Contains(q) || (t.Description != null && t.Description.ToLower().Contains(q))).Take(5);
        foreach(var t in filteredTasks)
        {
            results.Add(new {
                type = "Task",
                title = t.Title,
                subtitle = t.IsCompleted ? "Completed" : "Pending",
                url = "/Task/Edit/" + t.Id,
                icon = "fa-solid fa-list-check",
                color = "text-blue-500"
            });
        }

        // 2. Search Events
        var events = await _sqlUow.Events.GetEventsForUserAsync(userId);
        var filteredEvents = events.Where(e => e.Title.ToLower().Contains(q) || (e.Description != null && e.Description.ToLower().Contains(q))).Take(5);
        foreach(var e in filteredEvents)
        {
            results.Add(new {
                type = "Event",
                title = e.Title,
                subtitle = e.StartDate.ToString("MMM dd, yyyy"),
                url = "/Event/Details/" + e.Id,
                icon = "fa-solid fa-calendar-day",
                color = "text-purple-500"
            });
        }

        // 3. Search Resumes (Job Apps)
        var resumes = await _sqlUow.Resumes.FindAsync(r => r.UserId == userId);
        var filteredResumes = resumes.Where(r => r.PositionTitle.ToLower().Contains(q) || r.Description.ToLower().Contains(q)).Take(5);
        foreach(var r in filteredResumes)
        {
            results.Add(new {
                type = "Job Application",
                title = r.PositionTitle,
                subtitle = r.Status.ToString(),
                url = "/Resume/Details/" + r.Id,
                icon = "fa-solid fa-briefcase",
                color = "text-emerald-500"
            });
        }

        // 4. Search Ideas
        var ideas = await _noSqlUow.Ideas.GetAllAsync();
        var userIdeas = ideas.Where(i => i.UserId == userId && (i.Title.ToLower().Contains(q) || (i.Icon != null && i.Icon.ToLower().Contains(q)))).Take(5);
        foreach(var i in userIdeas)
        {
            results.Add(new {
                type = "Idea Note",
                title = i.Title,
                subtitle = "Notes & Ideas",
                url = "/IdeaTracker/Details/" + i.Id,
                icon = string.IsNullOrEmpty(i.Icon) ? "fa-solid fa-lightbulb" : i.Icon,
                color = "text-amber-500"
            });
        }

        // 5. System Features
        var features = new[] {
            new { title = "Dashboard Builder", url = "/Dashboard/Manage", keywords = "dashboard, builder, home, view", icon = "fa-solid fa-table-columns", adminOnly = false },
            new { title = "Expenses", url = "/Expense", keywords = "money, cost, expense, finance", icon = "fa-solid fa-money-bill", adminOnly = false },
            new { title = "Debts", url = "/Debt", keywords = "debt, owe, lend, borrow", icon = "fa-solid fa-hand-holding-dollar", adminOnly = false },
            new { title = "Habits", url = "/Habit", keywords = "habit, daily, tracker", icon = "fa-solid fa-check-double", adminOnly = false },
            new { title = "Support Tickets", url = "/Ticket", keywords = "support, ticket, help, bug", icon = "fa-solid fa-ticket", adminOnly = false },
            new { title = "Profile Settings", url = "/Profile", keywords = "profile, settings, user, account", icon = "fa-solid fa-user-gear", adminOnly = false },
            new { title = "File Explorer", url = "/FileExplorer", keywords = "file, explorer, wwwroot, admin", icon = "fa-solid fa-folder-tree", adminOnly = true }
        };

        foreach(var f in features)
        {
            if (f.title.ToLower().Contains(q) || f.keywords.Contains(q))
            {
                if (f.adminOnly && !_currentUser.IsAdmin) continue;
                results.Add(new {
                    type = "System Feature",
                    title = f.title,
                    subtitle = "App Navigation",
                    url = f.url,
                    icon = f.icon,
                    color = "text-slate-500"
                });
            }
        }

        return Json(results);
    }
}
