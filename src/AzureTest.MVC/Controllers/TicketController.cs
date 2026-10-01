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

namespace AzureTest.MVC.Controllers;

[Authorize]
public class TicketController : Controller
{
    private readonly ITicketQueryUnit _ticketQuery;
    private readonly ITicketCommandUnit _ticketCommand;
    private readonly IUserQueryUnit _userQuery;
    private readonly ICurrentUser _currentUser;

    public TicketController(
        ITicketQueryUnit ticketQuery,
        ITicketCommandUnit ticketCommand,
        IUserQueryUnit userQuery,
        ICurrentUser currentUser)
    {
        _ticketQuery = ticketQuery;
        _ticketCommand = ticketCommand;
        _userQuery = userQuery;
        _currentUser = currentUser;
    }

    public async Task<IActionResult> Index(
        string? status = null,
        string? priority = null,
        string? sortBy = null,
        string? sortOrder = null,
        int? page = null,
        int? pageNumber = null,
        int pageSize = 10)
    {
        bool isAdmin = _currentUser.Roles.Contains("Admin");
        var tickets = await _ticketQuery.GetAllTickets.HandleAsync(new GetAllTicketsQuery(isAdmin ? null : _currentUser.Id), default);

        IEnumerable<TicketDto> query = tickets;

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(t => t.Status == parsedStatus);
        }

        if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<TicketPriority>(priority, true, out var parsedPriority))
        {
            query = query.Where(t => t.Priority == parsedPriority);
        }

        bool isDesc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        query = (sortBy?.ToLower()) switch
        {
            "subject" => isDesc ? query.OrderByDescending(t => t.Subject) : query.OrderBy(t => t.Subject),
            "priority" => isDesc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "status" => isDesc ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "date" => isDesc ? query.OrderByDescending(t => t.CreatedOn) : query.OrderBy(t => t.CreatedOn),
            _ => (sortOrder != null && !isDesc) ? query.OrderBy(t => t.CreatedOn) : query.OrderByDescending(t => t.CreatedOn)
        };

        var ticketList = query.ToList();
        int totalCount = ticketList.Count;
        int currentPage = page ?? pageNumber ?? 1;
        if (currentPage < 1) currentPage = 1;
        if (pageSize < 1) pageSize = 10;

        var items = ticketList.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
        var pagedResult = new PagedResult<TicketDto>(items, totalCount, currentPage, pageSize);

        ViewBag.IsAdmin = isAdmin;
        ViewBag.Status = status;
        ViewBag.Priority = priority;
        ViewBag.SortBy = sortBy ?? "date";
        ViewBag.SortOrder = sortOrder ?? (sortBy == "subject" || sortBy == "priority" || sortBy == "status" ? "asc" : "desc");
        ViewBag.PageSize = pageSize;

        return View(pagedResult);
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

        var creator = await _userQuery.GetUserById.HandleAsync(new GetUserByIdQuery(ticket.UserId), default);

        ViewBag.IsAdmin = isAdmin;
        ViewBag.Creator = creator;
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
