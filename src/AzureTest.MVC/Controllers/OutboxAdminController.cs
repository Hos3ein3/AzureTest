using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.MVC.ViewModels.Outbox;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
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

    [HttpPost("Delete")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Outbox Message")]
    [Description("Delete a specific transactional Outbox message.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _uow.OutboxMessages.DeleteAsync(id, cancellationToken);
        if (deleted)
            TempData["Success"] = "Outbox message deleted successfully.";
        else
            TempData["Error"] = "Outbox message could not be found or deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("DeleteByDateRange")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Outbox Messages by Date Range")]
    [Description("Delete transactional Outbox messages within a specified date range.")]
    public async Task<IActionResult> DeleteByDateRange(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc) : null;
        DateTime? toUtc = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

        var count = await _uow.OutboxMessages.DeleteByDateRangeAsync(fromUtc, toUtc, cancellationToken);
        TempData["Success"] = $"{count} Outbox message(s) deleted within the specified timeframe.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Inbox/Delete")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Inbox Message")]
    [Description("Delete a specific deduplicated Inbox message.")]
    public async Task<IActionResult> DeleteInbox(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _uow.InboxMessages.DeleteAsync(id, cancellationToken);
        if (deleted)
            TempData["Success"] = "Inbox message deleted successfully.";
        else
            TempData["Error"] = "Inbox message could not be found or deleted.";

        return RedirectToAction(nameof(Inbox));
    }

    [HttpPost("Inbox/DeleteByDateRange")]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Inbox Messages by Date Range")]
    [Description("Delete deduplicated Inbox messages within a specified date range.")]
    public async Task<IActionResult> DeleteInboxByDateRange(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken)
    {
        DateTime? fromUtc = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc) : null;
        DateTime? toUtc = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

        var count = await _uow.InboxMessages.DeleteByDateRangeAsync(fromUtc, toUtc, cancellationToken);
        TempData["Success"] = $"{count} Inbox message(s) deleted within the specified timeframe.";
        return RedirectToAction(nameof(Inbox));
    }
}
