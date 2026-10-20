using AzureTest.Application.Abstractions;


using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.AspNetCore.Authorization;
using AzureTest.Application.Events;
using AzureTest.Application.Events.Queries;
using AzureTest.Application.Events.Commands;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Events;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Event")]
[Description("Manage and control Event operations.")]
    public class EventController : Controller
{
    private readonly IEventQueryUnit _queryUnit;
    private readonly IEventCommandUnit _commandUnit;
    private readonly ICurrentUser _currentUser;

    public EventController(IEventQueryUnit queryUnit, IEventCommandUnit commandUnit, ICurrentUser currentUser)
    {
        _queryUnit = queryUnit;
        _commandUnit = commandUnit;
        _currentUser = currentUser;
    }

    [DisplayName("Index")]
    [Description("View a list of Event.")]
    public async Task<IActionResult> Index([FromQuery] GetAllEventsFilters filters, CancellationToken cancellationToken)
    {
        filters ??= new GetAllEventsFilters();
        var userId = _currentUser.Id;
        var pagedResult = await _queryUnit.GetAllEvents.HandleAsync(new GetAllEventsQuery(filters, userId), cancellationToken);

        ViewBag.Filters = filters;
        return View(pagedResult);
    }

    [DisplayName("Create")]
    [Description("Create a new Event.")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Event.")]
    public async Task<IActionResult> Create(CreateEventCommand command)
    {
        if (ModelState.IsValid)
        {
            await _commandUnit.CreateEvent.HandleAsync(command, default);
            return RedirectToAction(nameof(Index));
        }
        return View(command);
    }

    [DisplayName("Edit")]
    [Description("Modify an existing Event.")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var ev = await _queryUnit.GetEventById.HandleAsync(new GetEventByIdQuery(id, _currentUser.Id), default);
        if (ev == null) return NotFound();

        var command = new UpdateEventCommand(ev.Id, ev.Title, ev.Description, ev.StartDate, ev.EndDate, ev.IsAllDay, ev.Color);
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Edit")]
    [Description("Modify an existing Event.")]
    public async Task<IActionResult> Edit(Guid id, UpdateEventCommand command)
    {
        if (id != command.Id) return NotFound();

        if (ModelState.IsValid)
        {
            await _commandUnit.UpdateEvent.HandleAsync(command, default);
            return RedirectToAction(nameof(Index));
        }
        return View(command);
    }

    [DisplayName("Details")]
    [Description("View details of a Event.")]
    public async Task<IActionResult> Details(Guid id)
    {
        var ev = await _queryUnit.GetEventById.HandleAsync(new GetEventByIdQuery(id, _currentUser.Id), default);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Confirmed")]
    [Description("Delete a Event.")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _commandUnit.DeleteEvent.HandleAsync(new DeleteEventCommand(id), default);
        return RedirectToAction(nameof(Index));
    }
}
