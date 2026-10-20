using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Pomodoro;
using AzureTest.Application.Pomodoro.Commands.AbandonPomodoroSession;
using AzureTest.Application.Pomodoro.Commands.CompletePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.CreatePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.DeletePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.UpdatePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.UpdatePomodoroSetting;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroAnalytics;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroSessions;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroSetting;
using AzureTest.Application.Tasks;
using AzureTest.Application.Tasks.Queries;
using AzureTest.Contracts.DTOs.Pomodoro;
using AzureTest.MVC.Extensions;
using AzureTest.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Pomodoro")]
[Description("Manage Pomodoro focus sessions, timer themes, and productivity analytics.")]
public class PomodoroController : Controller
{
    private readonly IPomodoroCommandUnit _commands;
    private readonly IPomodoroQueryUnit _queries;
    private readonly ITaskQueryUnit _taskQuery;
    private readonly IValidationService _validationService;
    private readonly ICurrentUser _currentUser;

    public PomodoroController(
        IPomodoroCommandUnit commands,
        IPomodoroQueryUnit queries,
        ITaskQueryUnit taskQuery,
        IValidationService validationService,
        ICurrentUser currentUser)
    {
        _commands = commands;
        _queries = queries;
        _taskQuery = taskQuery;
        _validationService = validationService;
        _currentUser = currentUser;
    }

    [HttpGet]
    [DisplayName("Pomodoro Timer")]
    [Description("Interactive Pomodoro focus timer with customizable skins.")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var settingResult = await _queries.GetSetting.HandleAsync(new GetPomodoroSettingQuery(_currentUser.Id), ct);
        var analyticsResult = await _queries.GetAnalytics.HandleAsync(new GetPomodoroAnalyticsQuery(_currentUser.Id), ct);
        var tasks = await _taskQuery.GetAllTasks.HandleAsync(new GetAllTasksQuery(_currentUser.Id), ct);

        var vm = new PomodoroIndexViewModel
        {
            Setting = settingResult.Value,
            TodayFocusMinutes = analyticsResult.Value?.TotalFocusMinutesToday ?? 0,
            TodayCompletedCount = analyticsResult.Value?.CompletedTodayCount ?? 0,
            CurrentStreak = analyticsResult.Value?.CurrentDailyStreak ?? 0,
            ActiveTasks = tasks.Where(t => !t.IsCompleted).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    [DisplayName("Pomodoro Analytics")]
    [Description("Productivity insights, charts, and focus session logs.")]
    public async Task<IActionResult> Analytics([FromQuery] PomodoroFilter filter, CancellationToken ct)
    {
        var analyticsResult = await _queries.GetAnalytics.HandleAsync(new GetPomodoroAnalyticsQuery(_currentUser.Id), ct);
        var sessionsResult = await _queries.GetSessions.HandleAsync(new GetPomodoroSessionsQuery(_currentUser.Id, filter), ct);
        var tasks = await _taskQuery.GetAllTasks.HandleAsync(new GetAllTasksQuery(_currentUser.Id), ct);

        var vm = new PomodoroAnalyticsViewModel
        {
            Analytics = analyticsResult.Value,
            RecentSessions = sessionsResult.Value ?? [],
            Filter = filter,
            Tasks = tasks
        };

        return View(vm);
    }

    [HttpPost]
    [DisplayName("Save Session")]
    [Description("Record a completed or abandoned pomodoro session.")]
    public async Task<IActionResult> SaveSession([FromBody] CreatePomodoroSessionDto dto, CancellationToken ct)
    {
        var validation = await _validationService.ValidateAsync(dto, ct);
        if (validation.IsFailure)
        {
            return BadRequest(new { success = false, errors = validation.ValidationErrors });
        }

        var command = new CreatePomodoroSessionCommand(_currentUser.Id, dto);
        var result = await _commands.CreateSession.HandleAsync(command, ct);

        if (result.IsFailure)
        {
            return BadRequest(new { success = false, message = result.Error });
        }

        return Json(new { success = true, id = result.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create Manual Session")]
    [Description("Manually record a pomodoro focus session.")]
    public async Task<IActionResult> CreateManualSession(CreatePomodoroSessionDto dto, CancellationToken ct)
    {
        var validation = await _validationService.ValidateAsync(dto, ct);
        if (validation.IsFailure)
        {
            return validation.ToModal(this, "createSessionModal", successRedirectAction: nameof(Analytics));
        }

        var command = new CreatePomodoroSessionCommand(_currentUser.Id, dto);
        var result = await _commands.CreateSession.HandleAsync(command, ct);
        return result.ToModal(this, "createSessionModal", successRedirectAction: nameof(Analytics));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit Session")]
    [Description("Update details of a pomodoro session.")]
    public async Task<IActionResult> EditSession(UpdatePomodoroSessionDto dto, CancellationToken ct)
    {
        var validation = await _validationService.ValidateAsync(dto, ct);
        if (validation.IsFailure)
        {
            return validation.ToModal(this, "editSessionModal", successRedirectAction: nameof(Analytics));
        }

        var command = new UpdatePomodoroSessionCommand(_currentUser.Id, dto);
        var result = await _commands.UpdateSession.HandleAsync(command, ct);
        return result.ToModal(this, "editSessionModal", successRedirectAction: nameof(Analytics));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Update Settings")]
    [Description("Update timer intervals, auto-start options, and skin preferences.")]
    public async Task<IActionResult> UpdateSettings(UpdatePomodoroSettingDto dto, CancellationToken ct)
    {
        var validation = await _validationService.ValidateAsync(dto, ct);
        if (validation.IsFailure)
        {
            return validation.ToFormResult(this, dto, nameof(Index));
        }

        var result = await _commands.UpdateSetting.HandleAsync(new UpdatePomodoroSettingCommand(_currentUser.Id, dto), ct);
        return result.ToFormResult(this, dto, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Session")]
    [Description("Soft-delete a pomodoro session entry from history.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _commands.DeleteSession.HandleAsync(new DeletePomodoroSessionCommand(_currentUser.Id, id), ct);
        return RedirectToAction(nameof(Analytics));
    }
}
