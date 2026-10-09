using AzureTest.Contracts.DTOs.Events;
using AzureTest.Contracts.DTOs.Debts;
using AzureTest.Application.Abstractions;


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
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Calendar")]
[Description("Manage and control Calendar operations.")]
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

    [DisplayName("Index")]
    [Description("View a list of Calendar.")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    [DisplayName("Get Events")]
    [Description("Execute Get Events operation for Calendar.")]
    public async Task<IActionResult> GetEvents(DateTime start, DateTime end)
    {
        var userId = _currentUser.Id;

        // 1. Get standard events
        var eventsResult = await _eventQuery.GetAllEvents.HandleAsync(new GetAllEventsQuery(new GetAllEventsFilters { PageSize = -1 }, userId), default);
        var events = eventsResult.Items;
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

        // 3. Get Debts
        var debtsResult = await _debtQuery.GetAll.HandleAsync(new GetAllDebtsQuery(new GetAllDebtsFilters { PageSize = -1 }, userId), default);
        var debts = debtsResult.Items;
        var debtEvents = debts
            .Select(d => new
            {
                debt = d,
                eventDate = d.PromissedReturnDate ?? d.DateOfReturn ?? d.DateOfBorrow
            })
            .Where(d => d.eventDate >= start && d.eventDate <= end)
            .Select(d => new
            {
                id = $"debt_{d.debt.Id}",
                title = d.debt.Amount > 0 ? "Debt (Owe)" : "Debt (Owed)",
                start = d.eventDate.ToString("O"),
                end = d.eventDate.ToString("O"),
                allDay = true,
                color = "#ef4444", // red
                type = "debt"
            });
            
        calendarEvents.AddRange(taskEvents);
        calendarEvents.AddRange(debtEvents);

        return Json(calendarEvents);
    }
}
