using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.MVC.ViewModels.AuditLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Audit Log")]
[Description("Inspect and monitor immutable audit trail of commands and queries.")]
public class AuditLogController : Controller
{
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUser _currentUser;
    private readonly ITenantProvider _tenantProvider;

    public AuditLogController(
        IAuditLogService auditLogService,
        ICurrentUser currentUser,
        ITenantProvider tenantProvider)
    {
        _auditLogService = auditLogService;
        _currentUser = currentUser;
        _tenantProvider = tenantProvider;
    }

    [HttpGet]
    [DisplayName("Audit Trail")]
    [Description("View audit log records filtered by permissions and tenant scope.")]
    public async Task<IActionResult> Index(
        string? actionType,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var isAdmin = _currentUser.IsAdmin;
        var currentUserId = _currentUser.Id;
        var currentTenantId = _tenantProvider.CurrentTenantId;

        var (logs, totalCount) = await _auditLogService.GetLogsAsync(
            isAdmin,
            currentUserId,
            currentTenantId,
            actionType,
            searchTerm,
            pageNumber,
            pageSize,
            cancellationToken);

        var vm = new AuditLogListViewModel
        {
            Logs = logs,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActionType = actionType,
            SearchTerm = searchTerm,
            IsAdmin = isAdmin,
            IsImpersonating = _tenantProvider.IsAdminImpersonating,
            CurrentTenantId = currentTenantId,
            CurrentUserId = currentUserId
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [DisplayName("Delete Audit Log")]
    [Description("Delete a specific audit log entry.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _auditLogService.DeleteLogByIdAsync(id, cancellationToken);
        if (deleted > 0)
            TempData["Success"] = "Audit log record deleted successfully.";
        else
            TempData["Error"] = "Audit log record could not be deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [DisplayName("Delete Logs by User")]
    [Description("Delete all audit logs for a specific user.")]
    public async Task<IActionResult> DeleteByUser(Guid userId, CancellationToken cancellationToken = default)
    {
        var deleted = await _auditLogService.DeleteLogsByUserAsync(userId, cancellationToken);
        TempData["Success"] = $"{deleted} audit log record(s) deleted for user.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [DisplayName("Delete Logs by Date Range")]
    [Description("Delete audit logs within a specific date range.")]
    public async Task<IActionResult> DeleteByDateRange(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        DateTime? fromUtc = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc) : null;
        DateTime? toUtc = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

        var deleted = await _auditLogService.DeleteLogsByDateRangeAsync(fromUtc, toUtc, cancellationToken);
        TempData["Success"] = $"{deleted} audit log record(s) deleted within specified timeframe.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin")]
    [DisplayName("Purge All Logs")]
    [Description("Permanently purge all audit log records.")]
    public async Task<IActionResult> PurgeAll(CancellationToken cancellationToken = default)
    {
        var deleted = await _auditLogService.PurgeAllLogsAsync(cancellationToken);
        TempData["Success"] = $"All audit log records purged successfully ({deleted} total).";
        return RedirectToAction(nameof(Index));
    }
}
