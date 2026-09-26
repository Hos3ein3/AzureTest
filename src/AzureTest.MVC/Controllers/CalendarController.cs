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
    private readonly ICurrentUser _currentUser;

    public CalendarController(
        IEventQueryUnit eventQuery,
        IEventCommandUnit eventCommand,
        ITaskQueryUnit taskQuery,
        IResumeQueryUnit resumeQuery,
        ICurrentUser currentUser)
    {
        _eventQuery = eventQuery;
        _eventCommand = eventCommand;
        _taskQuery = taskQuery;
        _resumeQuery = resumeQuery;
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

        // 2. Get Tasks with DueDate
        var tasks = await _taskQuery.GetAllTasks.HandleAsync(new GetAllTasksQuery(userId), default);
        var taskEvents = tasks
            .Where(t => t.DueDate.HasValue && t.DueDate.Value >= start && t.DueDate.Value <= end)
            .Select(t => new
            {
                id = $"task_{t.Id}",
                title = $"Task: {t.Title}",
                start = t.DueDate!.Value.ToString("O"),
                end = t.DueDate!.Value.ToString("O"),
                allDay = true,
                color = t.IsCompleted ? "#10b981" : "#f59e0b", // green if done, yellow if pending
                type = "task"
            });

        // 3. Get Resume Interviews
        // Note: Resumes don't have an InterviewDate field currently in the model, 
        // we'll just use CreatedOn for demonstration if Status is Interview, or we can skip this if they don't have dates.
        // For now, let's just combine events and tasks.
        
        calendarEvents.AddRange(taskEvents);

        return Json(calendarEvents);
    }
}
