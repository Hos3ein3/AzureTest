using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Events;
using AzureTest.Application.Events.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Resumes;
using AzureTest.Application.Resumes.Queries.GetAllResumes;
using AzureTest.Application.Tasks;
using AzureTest.Application.Tasks.Queries;
using AzureTest.Application.Debts.Interfaces;
using AzureTest.Application.Debts.Queries;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly IEventQueryUnit _eventQuery;
    private readonly IEventCommandUnit _eventCommand;
    private readonly ITaskQueryUnit _taskQuery;
    private readonly IResumeQueryUnit _resumeQuery;
    private readonly IDebtQueryUnit _debtQuery;
    private readonly ICurrentUser _currentUser;

    public CalendarController(
        IEventQueryUnit eventQuery,
        IEventCommandUnit eventCommand,
        ITaskQueryUnit taskQuery,
        IResumeQueryUnit resumeQuery,
        IDebtQueryUnit debtQuery,
        ICurrentUser currentUser)
    {
        _eventQuery = eventQuery;
        _eventCommand = eventCommand;
        _taskQuery = taskQuery;
        _resumeQuery = resumeQuery;
        _debtQuery = debtQuery;
        _currentUser = currentUser;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(DateTime start, DateTime end)
    {
        var userId = _currentUser.Id;

        // 1. Get standard events
        var events = await _eventQuery.GetAllEvents.HandleAsync(new GetAllEventsQuery(userId), default);
        var calendarEvents = events
            .Where(e => e.StartDate >= start && e.EndDate <= end)
            .Select(e => new
            {
                id = $"evt_{e.Id}",
                title = e.Title,
                start = e.StartDate.ToString("O"),
                end = e.EndDate.ToString("O"),
                allDay = e.IsAllDay,
                color = e.Color ?? "#3b82f6", // default blue
                type = "event"
            }).ToList();

        // 2. Get Tasks with StartDate/EndDate
        var tasks = await _taskQuery.GetAllTasks.HandleAsync(new GetAllTasksQuery(userId), default);
        var taskEvents = tasks
            .Where(t => (t.StartDate.HasValue && t.StartDate.Value >= start && t.StartDate.Value <= end) ||
                        (t.EndDate.HasValue && t.EndDate.Value >= start && t.EndDate.Value <= end) ||
                        (t.StartDate.HasValue && t.EndDate.HasValue && t.StartDate.Value <= start && t.EndDate.Value >= end))
            .Select(t => new
            {
                id = $"task_{t.Id}",
                title = $"Task: {t.Title}",
                start = (t.StartDate ?? t.EndDate)!.Value.ToString("O"),
                end = (t.EndDate ?? t.StartDate)!.Value.ToString("O"),
                allDay = true,
                color = t.IsCompleted ? "#10b981" : "#f59e0b", // green if done, yellow if pending
                type = "task"
            });

        // 3. Get Debts with PromissedReturnDate
        var debts = await _debtQuery.GetAll.HandleAsync(new GetAllDebtsQuery(userId), default);
        var debtEvents = debts
            .Where(d => d.PromissedReturnDate.HasValue && d.PromissedReturnDate.Value >= start && d.PromissedReturnDate.Value <= end)
            .Select(d => new
            {
                id = $"debt_{d.Id}",
                title = $"Debt: {d.DebtPersonName} ({(d.Amount > 0 ? "Owe" : "Owed")})",
                start = d.PromissedReturnDate!.Value.ToString("O"),
                end = d.PromissedReturnDate!.Value.ToString("O"),
                allDay = true,
                color = "#ef4444", // red
                type = "debt"
            });
            
        calendarEvents.AddRange(taskEvents);
        calendarEvents.AddRange(debtEvents);

        return Json(calendarEvents);
    }
}
