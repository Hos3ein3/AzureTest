using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Infrastructure.Services;
using AzureTest.MVC.ViewModels.Tenants;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AzureTest.MVC.Controllers;

[Authorize(Roles = "Admin")]
[DisplayName("Tenant Management")]
[Description("Switch active tenant workspace for administrative oversight.")]
public class TenantController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly InfrastructureFeatureOptions _featureOptions;
    private readonly ILogger<TenantController> _logger;

    public TenantController(
        UserManager<ApplicationUser> userManager,
        ITenantProvider tenantProvider,
        IOptions<InfrastructureFeatureOptions> featureOptions,
        ILogger<TenantController> logger)
    {
        _userManager = userManager;
        _tenantProvider = tenantProvider;
        _featureOptions = featureOptions.Value;
        _logger = logger;
    }

    [HttpGet]
    [DisplayName("Tenant Overview")]
    [Description("View all registered tenants and manage active tenant context.")]
    public async Task<IActionResult> Index(string? searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var currentTenantId = _tenantProvider.CurrentTenantId;
        var realAdminId = _tenantProvider.RealUserId;
        var isImpersonating = _tenantProvider.IsAdminImpersonating;

        string? currentTenantName = null;
        string? currentTenantEmail = null;

        if (currentTenantId != Guid.Empty)
        {
            var activeTenantUser = await _userManager.FindByIdAsync(currentTenantId.ToString());
            if (activeTenantUser != null)
            {
                currentTenantName = activeTenantUser.FullName;
                currentTenantEmail = activeTenantUser.Email;
            }
        }

        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new TenantItemViewModel
            {
                TenantId = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email ?? string.Empty,
                UserName = u.UserName ?? string.Empty,
                IsActiveTenant = u.Id == currentTenantId,
                IsCurrentAdmin = u.Id == realAdminId
            })
            .ToListAsync();

        var vm = new TenantManagementViewModel
        {
            IsMultiTenancyEnabled = _tenantProvider.IsMultiTenancyEnabled,
            CurrentTenantId = currentTenantId,
            RealAdminUserId = realAdminId,
            IsAdminImpersonating = isImpersonating,
            CurrentTenantName = currentTenantName,
            CurrentTenantEmail = currentTenantEmail,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Tenants = users
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Switch Tenant")]
    [Description("Switch active tenant context for administrative impersonation.")]
    public async Task<IActionResult> SwitchTenant([FromForm] Guid targetTenantId, [FromForm] string? returnUrl)
    {
        if (!_tenantProvider.IsMultiTenancyEnabled)
        {
            TempData["Error"] = "Multi-tenancy feature is currently disabled in system settings.";
            return RedirectToAction(nameof(Index));
        }

        if (targetTenantId == Guid.Empty)
        {
            TempData["Error"] = "Invalid tenant identifier provided.";
            return RedirectToAction(nameof(Index));
        }

        var targetUser = await _userManager.FindByIdAsync(targetTenantId.ToString());
        if (targetUser == null)
        {
            TempData["Error"] = "The specified tenant user was not found.";
            return RedirectToAction(nameof(Index));
        }

        // If target tenant is the admin themselves, remove impersonation cookie
        if (targetTenantId == _tenantProvider.RealUserId)
        {
            Response.Cookies.Delete(TenantProvider.AdminTenantCookieKey);
            TempData["Success"] = $"Returned to your personal admin workspace ({targetUser.Email}).";
            _logger.LogInformation("Admin user {AdminId} returned to personal workspace", _tenantProvider.RealUserId);
        }
        else
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                IsEssential = true
            };

            Response.Cookies.Append(TenantProvider.AdminTenantCookieKey, targetTenantId.ToString(), cookieOptions);
            TempData["Success"] = $"Active tenant switched to {targetUser.FullName} ({targetUser.Email}).";
            _logger.LogInformation("Admin user {AdminId} switched active tenant to {TargetTenantId} ({TargetEmail})",
                _tenantProvider.RealUserId, targetTenantId, targetUser.Email);
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Reset Tenant")]
    [Description("Clear active tenant impersonation and return to admin's own tenant.")]
    public IActionResult ResetTenant([FromForm] string? returnUrl)
    {
        Response.Cookies.Delete(TenantProvider.AdminTenantCookieKey);
        TempData["Success"] = "Tenant impersonation cleared. You are back in your own admin workspace.";
        _logger.LogInformation("Admin user {AdminId} reset tenant impersonation", _tenantProvider.RealUserId);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) && !returnUrl.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [DisplayName("Search Tenants")]
    [Description("Get tenant options for autocomplete and dropdown selectors.")]
    public async Task<IActionResult> GetTenants([FromQuery] string? search)
    {
        var currentTenantId = _tenantProvider.CurrentTenantId;
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.UserName != null && u.UserName.ToLower().Contains(term)) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term));
        }

        var results = await query
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Take(20)
            .Select(u => new
            {
                id = u.Id,
                fullName = (u.FirstName + " " + u.LastName).Trim(),
                email = u.Email,
                text = (u.FirstName + " " + u.LastName).Trim() + " (" + (u.Email ?? u.UserName) + ")",
                isCurrent = u.Id == currentTenantId
            })
            .ToListAsync();

        return Json(results);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Trigger Backfill Job")]
    [Description("Enqueue a background job to backfill TenantId for legacy database records in PostgreSQL and MongoDB.")]
    public IActionResult TriggerBackfillJob([FromServices] Hangfire.IBackgroundJobClient backgroundJobClient)
    {
        backgroundJobClient.Enqueue<AzureTest.Application.Interfaces.Services.ITenantBackfillJob>(
            job => job.ExecuteAsync(CancellationToken.None));

        TempData["Success"] = "TenantId backfill background job has been enqueued in Hangfire. Track progress in Background Jobs dashboard.";
        return RedirectToAction(nameof(Index));
    }
}
