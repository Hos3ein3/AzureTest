using AzureTest.Application.Abstractions;


using System;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Tasks;
using AzureTest.Application.Tasks.Commands;
using AzureTest.Application.Tasks.Queries;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Task")]
[Description("Manage and control Task operations.")]
    public class TaskController : Controller
{
    private readonly ITaskQueryUnit _taskQuery;
    private readonly ITaskCommandUnit _taskCommand;
    private readonly ICurrentUser _currentUser;

    public TaskController(ITaskQueryUnit taskQuery, ITaskCommandUnit taskCommand, ICurrentUser currentUser)
    {
        _taskQuery = taskQuery;
        _taskCommand = taskCommand;
        _currentUser = currentUser;
    }

    [DisplayName("Index")]
    [Description("View a list of Task.")]
    public async Task<IActionResult> Index()
    {
        var tasks = await _taskQuery.GetAllTasks.HandleAsync(new GetAllTasksQuery(_currentUser.Id), default);
        return View(tasks);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Task.")]
    public async Task<IActionResult> Create(string title, string? description, DateTime? startDate, DateTime? endDate, string? priority)
    {
        if (!string.IsNullOrWhiteSpace(title))
        {
            await _taskCommand.CreateTask.HandleAsync(new CreateTaskCommand(title, description, startDate, endDate, priority), default);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Toggle")]
    [Description("Execute Toggle operation for Task.")]
    public async Task<IActionResult> Toggle(Guid id)
    {
        await _taskCommand.ToggleTask.HandleAsync(new ToggleTaskCommand(id), default);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a Task.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _taskCommand.DeleteTask.HandleAsync(new DeleteTaskCommand(id), default);
        return RedirectToAction(nameof(Index));
    }
}
