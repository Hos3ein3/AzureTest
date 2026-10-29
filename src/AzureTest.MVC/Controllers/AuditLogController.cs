using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Users;
using AzureTest.Application.Users.Queries.GetAllUsers;
using AzureTest.Contracts.DTOs.Users;
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
    private readonly IUserQueryUnit _userQueries;

    public AuditLogController(
        IAuditLogService auditLogService,
        ICurrentUser currentUser,
        ITenantProvider tenantProvider,
        IUserQueryUnit userQueries)
    {
        _auditLogService = auditLogService;
        _currentUser = currentUser;
        _tenantProvider = tenantProvider;
        _userQueries = userQueries;
    }

    private bool CheckIsAdmin() => _currentUser.IsAdmin || User.IsInRole("Admin");

    [HttpGet]
    [DisplayName("Audit Trail")]
    [Description("View audit log records filtered by permissions and tenant scope.")]
    public async Task<IActionResult> Index(
        string? actionType,
        string? searchTerm,
        Guid? filterUserId,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var isAdmin = CheckIsAdmin();
        var currentUserId = _currentUser.Id;
        var currentTenantId = _tenantProvider.CurrentTenantId;

        IReadOnlyList<UserListDto> availableUsers = Array.Empty<UserListDto>();
        if (isAdmin)
        {
            try
            {
                var usersResult = await _userQueries.GetAllUsers.HandleAsync(
                    new GetAllUsersQuery(new GetAllUsersFilters { PageSize = 200 }),
                    cancellationToken);
                availableUsers = usersResult?.Items ?? Array.Empty<UserListDto>();
            }
            catch
            {
                // Fallback gracefully if query fails
            }
        }

        var (logs, totalCount) = await _auditLogService.GetLogsAsync(
            isAdmin,
            currentUserId,
            currentTenantId,
            actionType,
            searchTerm,
            pageNumber,
            pageSize,
            filterUserId,
            cancellationToken);

        var vm = new AuditLogListViewModel
        {
            Logs = logs,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            ActionType = actionType,
            SearchTerm = searchTerm,
            FilterUserId = filterUserId,
            AvailableUsers = availableUsers,
            IsAdmin = isAdmin,
            IsImpersonating = _tenantProvider.IsAdminImpersonating,
            CurrentTenantId = currentTenantId,
            CurrentUserId = currentUserId
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Audit Log")]
    [Description("Delete a specific audit log entry.")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        if (!CheckIsAdmin())
        {
            TempData["Error"] = "Unauthorized. Only administrators can delete audit logs.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await _auditLogService.DeleteLogByIdAsync(id, cancellationToken);
        if (deleted > 0)
            TempData["Success"] = "Audit log record deleted successfully.";
        else
            TempData["Error"] = "Audit log record could not be deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Logs by User")]
    [Description("Delete all audit logs for a specific user.")]
    public async Task<IActionResult> DeleteByUser(Guid userId, CancellationToken cancellationToken = default)
    {
        if (!CheckIsAdmin())
        {
            TempData["Error"] = "Unauthorized. Only administrators can delete audit logs.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await _auditLogService.DeleteLogsByUserAsync(userId, cancellationToken);
        TempData["Success"] = $"{deleted} audit log record(s) deleted for user.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Logs by Date Range")]
    [Description("Delete audit logs within a specific date range.")]
    public async Task<IActionResult> DeleteByDateRange(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        if (!CheckIsAdmin())
        {
            TempData["Error"] = "Unauthorized. Only administrators can delete audit logs.";
            return RedirectToAction(nameof(Index));
        }

        DateTime? fromUtc = fromDate.HasValue ? DateTime.SpecifyKind(fromDate.Value.Date, DateTimeKind.Utc) : null;
        DateTime? toUtc = toDate.HasValue ? DateTime.SpecifyKind(toDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

        var deleted = await _auditLogService.DeleteLogsByDateRangeAsync(fromUtc, toUtc, cancellationToken);
        TempData["Success"] = $"{deleted} audit log record(s) deleted within specified timeframe.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Purge All Logs")]
    [Description("Permanently purge all audit log records.")]
    public async Task<IActionResult> PurgeAll(CancellationToken cancellationToken = default)
    {
        if (!CheckIsAdmin())
        {
            TempData["Error"] = "Unauthorized. Only administrators can delete audit logs.";
            return RedirectToAction(nameof(Index));
        }

        var deleted = await _auditLogService.PurgeAllLogsAsync(cancellationToken);
        TempData["Success"] = $"All audit log records purged successfully ({deleted} total).";
        return RedirectToAction(nameof(Index));
    }
}
