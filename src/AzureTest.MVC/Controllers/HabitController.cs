using AzureTest.Application.Abstractions;


using System;
using System.Threading.Tasks;
using AzureTest.Application.Habits;
using AzureTest.Application.Habits.Commands;
using AzureTest.Application.Habits.Queries;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Habit.")]
    public async Task<IActionResult> Create(string name, string? description, HabitType type, HabitFrequency frequency, double goal)
    {
        if (ModelState.IsValid)
        {
            await _habitCommand.CreateHabit.HandleAsync(new CreateHabitCommand(name, description, type, frequency, goal), default);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Log")]
    [Description("Execute Log operation for Habit.")]
    public async Task<IActionResult> Log(Guid id, bool booleanValue, double numericValue, string? notes)
    {
        await _habitCommand.LogHabit.HandleAsync(new LogHabitCommand(id, DateTime.UtcNow, booleanValue, numericValue, notes), default);
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
