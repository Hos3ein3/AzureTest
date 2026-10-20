using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Pomodoro;
using AzureTest.Application.Pomodoro.Commands.AbandonPomodoroSession;
using AzureTest.Application.Pomodoro.Commands.CompletePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.CreatePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.DeletePomodoroSession;
using AzureTest.Application.Pomodoro.Commands.UpdatePomodoroSetting;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroAnalytics;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroSessions;
using AzureTest.Application.Pomodoro.Queries.GetPomodoroSetting;
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
    private readonly IValidationService _validationService;
    private readonly ICurrentUser _currentUser;

    public PomodoroController(
        IPomodoroCommandUnit commands,
        IPomodoroQueryUnit queries,
        IValidationService validationService,
        ICurrentUser currentUser)
    {
        _commands = commands;
        _queries = queries;
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

        var vm = new PomodoroIndexViewModel
        {
            Setting = settingResult.Value,
            TodayFocusMinutes = analyticsResult.Value?.TotalFocusMinutesToday ?? 0,
            TodayCompletedCount = analyticsResult.Value?.CompletedTodayCount ?? 0,
            CurrentStreak = analyticsResult.Value?.CurrentDailyStreak ?? 0
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

        var vm = new PomodoroAnalyticsViewModel
        {
            Analytics = analyticsResult.Value,
            RecentSessions = sessionsResult.Value ?? [],
            Filter = filter
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
