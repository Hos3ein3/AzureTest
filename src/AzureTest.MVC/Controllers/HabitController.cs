using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Habits;
using AzureTest.Application.Habits.Commands;
using AzureTest.Application.Habits.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Habit")]
[Description("Manage and control Habit operations.")]
public class HabitController : Controller
{
    private readonly IHabitQueryUnit _habitQuery;
    private readonly IHabitCommandUnit _habitCommand;
    private readonly ICurrentUser _currentUser;

    public HabitController(IHabitQueryUnit habitQuery, IHabitCommandUnit habitCommand, ICurrentUser currentUser)
    {
        _habitQuery = habitQuery;
        _habitCommand = habitCommand;
        _currentUser = currentUser;
    }

    [DisplayName("Index")]
    [Description("View a list of Habit.")]
    public async Task<IActionResult> Index()
    {
        var habits = await _habitQuery.GetAllHabits.HandleAsync(new GetAllHabitsQuery(_currentUser.Id), default);
        return View(habits);
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View details, visual charts and logs for a Habit.")]
    public async Task<IActionResult> Details(Guid id)
    {
        var habit = await _habitQuery.GetHabitById.HandleAsync(new GetHabitByIdQuery(id, _currentUser.Id), default);
        if (habit == null)
        {
            return NotFound();
        }
        return View(habit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Habit.")]
    public async Task<IActionResult> Create(
        string name,
        string? description,
        HabitType type,
        HabitDirection direction = HabitDirection.AtLeast,
        double? target = null,
        double? goal = null,
        string? schedule = null,
        HabitFrequency frequency = HabitFrequency.Daily,
        string? timeZone = "UTC",
        bool isPrivate = false,
        string? typeConfig = null)
    {
        // Support backward-compatible 'goal' if target was omitted
        double? finalTarget = target ?? goal;
        if (!finalTarget.HasValue && type == HabitType.Boolean) finalTarget = 1;

        if (ModelState.IsValid)
        {
            var command = new CreateHabitCommand(
                Name: name,
                Description: description,
                Type: type,
                Direction: direction,
                Target: finalTarget,
                Schedule: schedule ?? frequency.ToString(),
                Frequency: frequency,
                TimeZone: timeZone ?? "UTC",
                IsPrivate: isPrivate,
                TypeConfig: typeConfig
            );

            await _habitCommand.CreateHabit.HandleAsync(command, default);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Edit an existing Habit.")]
    public async Task<IActionResult> Edit(
        Guid id,
        string name,
        string? description,
        HabitType type,
        HabitDirection direction = HabitDirection.AtLeast,
        double? target = null,
        double? goal = null,
        string? schedule = null,
        HabitFrequency frequency = HabitFrequency.Daily,
        string? timeZone = "UTC",
        bool isArchived = false,
        bool isPrivate = false,
        string? typeConfig = null)
    {
        double? finalTarget = target ?? goal;

        if (ModelState.IsValid)
        {
            var command = new UpdateHabitCommand(
                Id: id,
                Name: name,
                Description: description,
                Type: type,
                Direction: direction,
                Target: finalTarget,
                Schedule: schedule ?? frequency.ToString(),
                Frequency: frequency,
                TimeZone: timeZone ?? "UTC",
                IsArchived: isArchived,
                IsPrivate: isPrivate,
                TypeConfig: typeConfig
            );

            await _habitCommand.UpdateHabit.HandleAsync(command, default);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Log")]
    [Description("Execute Log operation for Habit.")]
    public async Task<IActionResult> Log(
        Guid id,
        Guid? habitId,
        string? logDate,
        string? value,
        bool? booleanValue,
        double? numericValue,
        string? notes,
        string? returnUrl = null)
    {
        var targetId = habitId ?? id;
        DateOnly? parsedDate = null;
        if (!string.IsNullOrWhiteSpace(logDate) && DateOnly.TryParse(logDate, CultureInfo.InvariantCulture, out var d))
        {
            parsedDate = d;
        }

        var command = new LogHabitCommand(
            HabitId: targetId,
            LocalDate: parsedDate,
            Value: value,
            NumericValue: numericValue,
            BooleanValue: booleanValue,
            Notes: notes
        );

        await _habitCommand.LogHabit.HandleAsync(command, default);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Undo")]
    [Description("Undo the latest log entry for a habit (e.g. counter).")]
    public async Task<IActionResult> Undo(Guid id, Guid? habitId, string? logDate, string? returnUrl = null)
    {
        var targetId = habitId ?? id;
        DateOnly? parsedDate = null;
        if (!string.IsNullOrWhiteSpace(logDate) && DateOnly.TryParse(logDate, CultureInfo.InvariantCulture, out var d))
        {
            parsedDate = d;
        }

        await _habitCommand.UndoHabitLog.HandleAsync(new UndoHabitLogCommand(targetId, parsedDate), default);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete")]
    [Description("Delete a Habit.")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _habitCommand.DeleteHabit.HandleAsync(new DeleteHabitCommand(id), default);
        return RedirectToAction(nameof(Index));
    }
}
