using System;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Tickets;
using AzureTest.Application.Tickets.Commands;
using AzureTest.Application.Tickets.Queries;
using AzureTest.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class TicketController : Controller
{
    private readonly ITicketQueryUnit _ticketQuery;
    private readonly ITicketCommandUnit _ticketCommand;
    private readonly ICurrentUser _currentUser;

    public TicketController(ITicketQueryUnit ticketQuery, ITicketCommandUnit ticketCommand, ICurrentUser currentUser)
    {
        _ticketQuery = ticketQuery;
        _ticketCommand = ticketCommand;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index()
    {
        bool isAdmin = _currentUser.Roles.Contains("Admin");
        var tickets = await _ticketQuery.GetAllTickets.HandleAsync(new GetAllTicketsQuery(isAdmin ? null : _currentUser.Id), default);
        ViewBag.IsAdmin = isAdmin;
        return View(tickets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string subject, string content, TicketPriority priority)
    {
        if (ModelState.IsValid)
        {
            var ticketId = await _ticketCommand.CreateTicket.HandleAsync(new CreateTicketCommand(subject, content, priority), default);
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        bool isAdmin = _currentUser.Roles.Contains("Admin");
        var ticket = await _ticketQuery.GetTicketById.HandleAsync(new GetTicketByIdQuery(id, isAdmin ? null : _currentUser.Id), default);
        
        if (ticket == null) return NotFound();

        // Mark messages as read if the user is the creator
        if (ticket.UserId == _currentUser.Id)
        {
            await _ticketCommand.MarkTicketMessagesAsRead.HandleAsync(new MarkTicketMessagesAsReadCommand(id), default);
        }

        ViewBag.IsAdmin = isAdmin;
        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMessage(Guid ticketId, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            await _ticketCommand.AddTicketMessage.HandleAsync(new AddTicketMessageCommand(ticketId, content), default);
        }
        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStatus(Guid ticketId, TicketStatus status)
    {
        await _ticketCommand.UpdateTicketStatus.HandleAsync(new UpdateTicketStatusCommand(ticketId, status), default);
        return RedirectToAction(nameof(Details), new { id = ticketId });
    }
}
