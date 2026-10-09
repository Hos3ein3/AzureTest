using AzureTest.Application.Abstractions;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Tickets;
using AzureTest.Application.Tickets.Commands;
using AzureTest.Application.Tickets.Queries;
using AzureTest.Application.Users;
using AzureTest.Application.Users.Queries.GetUserById;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Tickets;
using AzureTest.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using AzureTest.MVC.Hubs;
using System.ComponentModel;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Ticket")]
[Description("Manage and control Ticket operations.")]
    public class TicketController : Controller
{
    private readonly ITicketQueryUnit _ticketQuery;
    private readonly ITicketCommandUnit _ticketCommand;
    private readonly IUserQueryUnit _userQuery;
    private readonly ICurrentUser _currentUser;
    private readonly IHubContext<NotificationHub> _hubContext;

    public TicketController(
        ITicketQueryUnit ticketQuery,
        ITicketCommandUnit ticketCommand,
        IUserQueryUnit userQuery,
        ICurrentUser currentUser, IHubContext<NotificationHub> hubContext)
    {
        _ticketQuery = ticketQuery;
        _ticketCommand = ticketCommand;
        _hubContext = hubContext;
        _userQuery = userQuery;
        _currentUser = currentUser;
    }

    [DisplayName("Index")]
    [Description("View a list of Ticket.")]
    public async Task<IActionResult> Index([FromQuery] GetAllTicketsFilters filters, [FromQuery] int? page, CancellationToken cancellationToken)
    {
        filters ??= new GetAllTicketsFilters();
        if (page.HasValue && page.Value > 0)
        {
            filters.PageNumber = page.Value;
        }

        bool isStaff = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("Ticket/UpdateStatus");
        var pagedResult = await _ticketQuery.GetAllTickets.HandleAsync(new GetAllTicketsQuery(filters, isStaff ? null : _currentUser.Id), cancellationToken);

        ViewBag.IsAdmin = isStaff;
        ViewBag.Filters = filters;

        return View(pagedResult);
    }

    [HttpGet]
    [DisplayName("Create")]
    [Description("Create a new Ticket.")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Create")]
    [Description("Create a new Ticket.")]
    public async Task<IActionResult> Create(string subject, string content, TicketPriority priority)
    {
        if (ModelState.IsValid)
        {
            var ticketId = await _ticketCommand.CreateTicket.HandleAsync(new CreateTicketCommand(subject, content, priority), default);
            await _hubContext.Clients.All.SendAsync("ReceiveTicketNotification", "New Ticket", subject);
            return RedirectToAction(nameof(Details), new { id = ticketId.Value });
        }
        return View();
    }

    [HttpGet]
    [DisplayName("Details")]
    [Description("View details of a Ticket.")]
    public async Task<IActionResult> Details(Guid id)
    {
        bool isStaff = _currentUser.Roles.Contains("Admin") || _currentUser.Permissions.Contains("Ticket/UpdateStatus");
        var ticket = await _ticketQuery.GetTicketById.HandleAsync(new GetTicketByIdQuery(id, isStaff ? null : _currentUser.Id), default);
        
        if (ticket == null) return NotFound();

        // Mark messages as read if the user is the creator
        if (ticket.UserId == _currentUser.Id)
        {
            await _ticketCommand.MarkTicketMessagesAsRead.HandleAsync(new MarkTicketMessagesAsReadCommand(id), default);
        }

        var creator = await _userQuery.GetUserById.HandleAsync(new GetUserByIdQuery(ticket.UserId), default);

        ViewBag.IsAdmin = isStaff;
        ViewBag.Creator = creator;
        return View(ticket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Add Message")]
    [Description("Create a new Ticket.")]
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
    
    [DisplayName("Update Status")]
    [Description("Modify an existing Ticket.")]
    public async Task<IActionResult> UpdateStatus(Guid ticketId, TicketStatus status)
    {
        await _ticketCommand.UpdateTicketStatus.HandleAsync(new UpdateTicketStatusCommand(ticketId, status), default);
        return RedirectToAction(nameof(Details), new { id = ticketId });
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Report Bug")]
    [Description("Automatically generate a bug ticket from an error page.")]
    public async Task<IActionResult> ReportBug(string requestId, string exceptionMessage, string innerExceptionMessage)
    {
        var subject = $"Automated Bug Report - Req: {requestId}";
        var content = $@"An automated bug report was generated.

Exception:
{exceptionMessage}

Inner Exception:
{innerExceptionMessage}";
        
        var ticketId = await _ticketCommand.CreateTicket.HandleAsync(new CreateTicketCommand(subject, content, TicketPriority.High), default);
        
        if (ticketId.IsSuccess)
        {
            TempData["SuccessMessage"] = "Bug report submitted successfully! Thank you.";
            await _hubContext.Clients.All.SendAsync("ReceiveTicketNotification", "New Ticket", subject);
            return RedirectToAction(nameof(Details), new { id = ticketId.Value });
        }
        
        return RedirectToAction("Index", "Home");
    }
}

