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
}
