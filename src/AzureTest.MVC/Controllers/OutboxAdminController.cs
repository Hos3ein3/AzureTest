using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.MVC.ViewModels.Outbox;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Outbox & Inbox")]
[Description("Administrative monitoring of transactional Outbox and Inbox messages.")]
[Route("OutboxAdmin")]
[Route("Outbox")]
public class OutboxAdminController : Controller
{
    private readonly IUnitOfWork _uow;

    public OutboxAdminController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    [HttpGet("Outbox")]
    [DisplayName("Outbox Messages")]
    [Description("View and filter transactional Outbox event messages.")]
    public async Task<IActionResult> Index(
        string? searchTerm,
        bool? isDone,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy = "OccurredOnUtc",
        bool sortDescending = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var (items, totalCount) = await _uow.OutboxMessages.GetPagedAsync(
            searchTerm,
            isDone,
            fromDate,
            toDate,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize,
            cancellationToken);

        // Fetch high-level metrics for admin dashboard cards
        var (_, pendingCount) = await _uow.OutboxMessages.GetPagedAsync(
            isDone: false,
            pageNumber: 1,
            pageSize: 1,
            ct: cancellationToken);

        var (_, processedCount) = await _uow.OutboxMessages.GetPagedAsync(
            isDone: true,
            pageNumber: 1,
            pageSize: 1,
            ct: cancellationToken);

        var vm = new OutboxMessageListViewModel
        {
            Messages = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsDone = isDone,
            FromDate = fromDate,
            ToDate = toDate,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PendingCount = pendingCount,
            ProcessedCount = processedCount,
            FailedCount = items.Count(m => m.RetryCount > 0 || !string.IsNullOrWhiteSpace(m.Error))
        };

        return View(vm);
    }

    [HttpGet("Inbox")]
    [DisplayName("Inbox Messages")]
    [Description("View and filter deduplicated Inbox event messages.")]
    public async Task<IActionResult> Inbox(
        string? searchTerm,
        DateTime? fromDate,
        DateTime? toDate,
        string? sortBy = "ReceivedOnUtc",
        bool sortDescending = true,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var (items, totalCount) = await _uow.InboxMessages.GetPagedAsync(
            searchTerm,
            fromDate,
            toDate,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize,
            cancellationToken);

        var vm = new InboxMessageListViewModel
        {
            Messages = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            FromDate = fromDate,
            ToDate = toDate,
            SortBy = sortBy,
            SortDescending = sortDescending,
            TotalProcessedCount = totalCount
        };

        return View(vm);
    }

    [HttpGet("OutboxPayload/{id:guid}")]
    public async Task<IActionResult> GetOutboxPayload(Guid id, CancellationToken cancellationToken)
    {
        var message = await _uow.OutboxMessages.GetByIdAsync(id, cancellationToken);
        if (message == null)
        {
            return NotFound(new { error = "Outbox message not found." });
        }

        return Json(new
        {
            id = message.Id,
            eventType = message.EventType,
            occurredOnUtc = message.OccurredOnUtc,
            processedOnUtc = message.ProcessedOnUtc,
            retryCount = message.RetryCount,
            error = message.Error,
            payload = message.Payload
        });
    }

    [HttpPost("RetryOutbox/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryOutbox(Guid id, CancellationToken cancellationToken)
    {
        var message = await _uow.OutboxMessages.GetByIdAsync(id, cancellationToken);
        if (message == null)
        {
            TempData["Error"] = "Outbox message not found.";
            return RedirectToAction(nameof(Index));
        }

        message.RetryCount = 0;
        message.Error = null;
        message.ProcessedOnUtc = null;
        _uow.OutboxMessages.Update(message);
        await _uow.SaveChangesAsync(cancellationToken);

        TempData["Success"] = $"Outbox message {id} reset for background re-dispatch.";
        return RedirectToAction(nameof(Index));
    }
}
