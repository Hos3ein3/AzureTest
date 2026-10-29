using System;
using System.Security.Claims;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Infrastructure.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AzureTest.Infrastructure.Services;

/// <summary>
/// Scoped service that determines the current tenant workspace based on the authenticated user,
/// feature flag configuration, and administrative impersonation cookie / header.
/// </summary>
public class TenantProvider : ITenantProvider
{
    public const string AdminTenantCookieKey = "Admin_Active_TenantId";
    public const string AdminTenantNameCookieKey = "Admin_Active_TenantName";
    public const string TenantHeaderKey = "X-Tenant-Id";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly InfrastructureFeatureOptions _featureOptions;

    public TenantProvider(
        IHttpContextAccessor httpContextAccessor,
        IOptions<InfrastructureFeatureOptions> featureOptions)
    {
        _httpContextAccessor = httpContextAccessor;
        _featureOptions = featureOptions.Value;
    }

    public bool IsMultiTenancyEnabled => _featureOptions.EnableMultiTenancy;

    public bool AllowImpersonationWrites => _featureOptions.AllowImpersonationWrites;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid RealUserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("sub")?.Value;

            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAdmin => User?.IsInRole("Admin") ?? false;

    public bool IsAdminImpersonating
    {
        get
        {
            if (!IsMultiTenancyEnabled || !IsAdmin)
                return false;

            var overrideTenantId = GetAdminOverrideTenantId();
            return overrideTenantId.HasValue && overrideTenantId.Value != Guid.Empty && overrideTenantId.Value != RealUserId;
        }
    }

    public string? ImpersonatedTenantName
    {
        get
        {
            if (!IsAdminImpersonating)
                return null;

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            if (httpContext.Request.Cookies.TryGetValue(AdminTenantNameCookieKey, out var name) && !string.IsNullOrWhiteSpace(name))
            {
                return name;
            }

            return null;
        }
    }

    public Guid CurrentTenantId
    {
        get
        {
            if (!IsMultiTenancyEnabled)
                return Guid.Empty;

            var realUserId = RealUserId;
            if (realUserId == Guid.Empty)
                return Guid.Empty;

            // Strict security isolation: Only Admins can switch/impersonate
            if (IsAdmin)
            {
                var overrideTenantId = GetAdminOverrideTenantId();
                if (overrideTenantId.HasValue && overrideTenantId.Value != Guid.Empty)
                {
                    return overrideTenantId.Value;
                }
            }

            // Normal users are strictly locked to their authenticated UserId
            return realUserId;
        }
    }

    private Guid? GetAdminOverrideTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Check header first (useful for API/AJAX calls)
        if (httpContext.Request.Headers.TryGetValue(TenantHeaderKey, out var headerValue) &&
            Guid.TryParse(headerValue, out var headerTenantId))
        {
            return headerTenantId;
        }

        // Check secure cookie
        if (httpContext.Request.Cookies.TryGetValue(AdminTenantCookieKey, out var cookieValue) &&
            Guid.TryParse(cookieValue, out var cookieTenantId))
        {
            return cookieTenantId;
        }

        return null;
    }
}
