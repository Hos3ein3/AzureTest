using System;
using System.Linq;
using System.Security.Claims;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    public const string ProfileImageClaim = "ProfileImage";
    public const string FirstNameClaim = "FirstName";
    public const string LastNameClaim = "LastName";
    public const string FullNameClaim = "FullName";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITenantProvider? _tenantProvider;

    public CurrentUser(
        IHttpContextAccessor httpContextAccessor, 
        IServiceProvider serviceProvider,
        ITenantProvider? tenantProvider = null)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _tenantProvider = tenantProvider;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid Id
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("sub")?.Value;

            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }
    }
    
    // Helper to get cached user session from Redis
    private AzureTest.Contracts.DTOs.Users.UserSessionCacheDto? GetCachedSession()
    {
        var id = Id;
        if (id == Guid.Empty) return null;
        
        var redis = _serviceProvider.GetService<IRedisService>();
        if (redis == null) return null;
        
        var cached = redis.GetAsync<AzureTest.Contracts.DTOs.Users.UserSessionCacheDto>($"User_Info_{id}").GetAwaiter().GetResult();
        if (cached != null) return cached;

        // Auto re-populate Redis cache from database if expired or missed
        try
        {
            var userManager = _serviceProvider.GetService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
            var sessionService = _serviceProvider.GetService<AzureTest.Application.Auth.Services.IAuthSessionService>();
            if (userManager != null && sessionService != null)
            {
                var userObj = userManager.FindByIdAsync(id.ToString()).GetAwaiter().GetResult();
                if (userObj != null)
                {
                    sessionService.CacheUserSessionAsync(userObj).GetAwaiter().GetResult();
                    return redis.GetAsync<AzureTest.Contracts.DTOs.Users.UserSessionCacheDto>($"User_Info_{id}").GetAwaiter().GetResult();
                }
            }
        }
        catch
        {
            // Cache repopulation failure should not throw
        }

        return null;
    }

    public string Email =>
        GetCachedSession()?.Email 
        ?? User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value
        ?? string.Empty;

    public string? PhoneNumber =>
        GetCachedSession()?.PhoneNumber
        ?? User?.FindFirst(ClaimTypes.MobilePhone)?.Value
        ?? User?.FindFirst("PhoneNumber")?.Value
        ?? User?.FindFirst("phone_number")?.Value;

    public string? ProfileImage =>
        GetCachedSession()?.ProfileImage
        ?? User?.FindFirst(ProfileImageClaim)?.Value
        ?? User?.FindFirst("profile_image")?.Value;

    public string FirstName =>
        GetCachedSession()?.FirstName
        ?? User?.FindFirst(FirstNameClaim)?.Value
        ?? User?.FindFirst(ClaimTypes.GivenName)?.Value
        ?? string.Empty;

    public string LastName =>
        GetCachedSession()?.LastName
        ?? User?.FindFirst(LastNameClaim)?.Value
        ?? User?.FindFirst(ClaimTypes.Surname)?.Value
        ?? string.Empty;

    public string FullName
    {
        get
        {
            var cached = GetCachedSession();
            if (cached != null && !string.IsNullOrWhiteSpace(cached.FullName)) return cached.FullName;

            var fullNameClaim = User?.FindFirst(FullNameClaim)?.Value;
            if (!string.IsNullOrWhiteSpace(fullNameClaim))
            {
                return fullNameClaim;
            }

            var first = FirstName;
            var last = LastName;
            var combined = $"{first} {last}".Trim();
            return string.IsNullOrWhiteSpace(combined) ? string.Empty : combined;
        }
    }

    public System.Collections.Generic.IReadOnlyList<string> Roles =>
        GetCachedSession()?.Roles
        ?? User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() 
        ?? new System.Collections.Generic.List<string>();

    public System.Collections.Generic.IReadOnlyList<string> DirectPermissions
    {
        get
        {
            var cached = GetCachedSession();
            if (cached != null) return cached.DirectPermissions;

            // Direct DB fallback if Redis is unavailable
            var id = Id;
            if (id != Guid.Empty)
            {
                var unitOfWork = _serviceProvider.GetService<AzureTest.Application.Interfaces.Data.IUnitOfWork>();
                if (unitOfWork != null)
                {
                    try
                    {
                        var directPerms = unitOfWork.UserPermissions.GetPermissionsByUserIdAsync(id).GetAwaiter().GetResult();
                        var list = new System.Collections.Generic.List<string>();
                        foreach (var up in directPerms.Where(x => x.IsGranted))
                        {
                            if (up.Permission != null)
                            {
                                var areaPrefix = !string.IsNullOrWhiteSpace(up.Permission.AreaName) ? $"{up.Permission.AreaName}/" : "";
                                list.Add($"{areaPrefix}{up.Permission.ControllerName}/{up.Permission.ActionName}");
                                list.Add($"{up.Permission.ControllerName}/{up.Permission.ActionName}");
                            }
                        }
                        return list.Distinct().ToList();
                    }
                    catch
                    {
                        // Fallback failure should not throw
                    }
                }
            }

            return new System.Collections.Generic.List<string>();
        }
    }

    public System.Collections.Generic.IReadOnlyList<string> RolePermissions =>
        GetCachedSession()?.RolePermissions
        ?? new System.Collections.Generic.List<string>();

    public System.Collections.Generic.IReadOnlyList<string> Permissions =>
        GetCachedSession()?.Permissions
        ?? new System.Collections.Generic.List<string>();

    public bool IsAdmin
    {
        get
        {
            return Roles.Contains("Admin");
        }
    }

    public bool IsReadOnly
    {
        get
        {
            var cached = GetCachedSession();
            if (cached != null) return cached.IsReadOnly;

            var id = Id;
            if (id != Guid.Empty)
            {
                var userManager = _serviceProvider.GetService<Microsoft.AspNetCore.Identity.UserManager<ApplicationUser>>();
                if (userManager != null)
                {
                    try
                    {
                        var user = userManager.FindByIdAsync(id.ToString()).GetAwaiter().GetResult();
                        return user?.IsReadOnly ?? false;
                    }
                    catch { }
                }
            }

            return false;
        }
    }

    public Guid TenantId => _tenantProvider?.IsMultiTenancyEnabled == true
        ? _tenantProvider.CurrentTenantId
        : Id;

    // ─── HasAccess overloads ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public bool HasAccess() => CheckAccess(_httpContextAccessor, null);

    /// <inheritdoc/>
    public bool HasAccess(string url) => CheckAccess(_httpContextAccessor, url);

    /// <inheritdoc/>
    public bool HasAccess(IHttpContextAccessor httpContextAccessor, string? url = null)
        => CheckAccess(httpContextAccessor, url);

    public bool BelongToCurrentUser(string entityUserId)
    {
        if (string.IsNullOrWhiteSpace(entityUserId)) return false;
        var effectiveId = _tenantProvider?.IsMultiTenancyEnabled == true ? TenantId : Id;
        return Guid.TryParse(entityUserId, out var parsed) && effectiveId.Equals(parsed);
    }

    public bool BelongToCurrentUser(Guid entityUserId)
    {
        var effectiveId = _tenantProvider?.IsMultiTenancyEnabled == true ? TenantId : Id;
        return effectiveId.Equals(entityUserId);
    }

    // ─── Core logic ─────────────────────────────────────────────────────────────

    private bool CheckAccess(IHttpContextAccessor accessor, string? url)
    {
        // Admin always bypasses all checks
        if (Roles.Contains("Admin")) return true;

        var httpContext = accessor.HttpContext;

        // Determine the target URL
        var targetUrl = url ?? httpContext?.Request.Path.Value ?? "/";
        if (string.IsNullOrWhiteSpace(targetUrl) || targetUrl == "/")
            targetUrl = "Home/Index";

        // Extract area, controller and action from URL
        var parts = targetUrl.TrimStart('/').Split('?')[0].Split('/');
        string? area = null;
        string controller;
        string action;

        if (parts.Length >= 3)
        {
            area = parts[0];
            controller = parts[1];
            action = parts[2];
        }
        else
        {
            controller = parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : "Home";
            action     = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]) ? parts[1] : "Index";
        }

        // Users with the "User" role get automatic access to reserved slugs (Home, Ticket, etc.)
        if (Roles.Contains("User") && httpContext != null)
        {
           
           var feature=httpContext.RequestServices
               .GetService<Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.InfrastructureFeatureOptions>>();
           if (feature!.Value.EnableUserAccessToDashboardSlugs)
           {
               var options = httpContext.RequestServices
                   .GetService<Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.DashboardSettings>>();
               var reserved = options?.Value.ReservedSlugs ?? new System.Collections.Generic.List<string>();
               if (reserved.Contains(controller, StringComparer.OrdinalIgnoreCase))
                   return true;
           }
           
        }

        // Fall back to explicit permission check: "Area/Controller/Action" or "Controller/Action"
        var fullKey = !string.IsNullOrWhiteSpace(area) ? $"{area}/{controller}/{action}" : null;
        var shortKey = $"{controller}/{action}";

        // 1. Direct User Permissions check (takes priority and bypasses roles)
        if (DirectPermissions.Any(p => (fullKey != null && p.Equals(fullKey, StringComparison.OrdinalIgnoreCase)) || p.Equals(shortKey, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // 2. Role / Combined permissions check
        return (fullKey != null && Permissions.Any(p => p.Equals(fullKey, StringComparison.OrdinalIgnoreCase)))
            || Permissions.Any(p => p.Equals(shortKey, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasDirectPermission(string permissionKey)
    {
        if (string.IsNullOrWhiteSpace(permissionKey)) return false;
        if (IsAdmin) return true;
        var cleanKey = permissionKey.TrimStart('/');
        return DirectPermissions.Any(p => p.Equals(cleanKey, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasPermission(string permissionKey)
    {
        if (string.IsNullOrWhiteSpace(permissionKey)) return false;
        if (IsAdmin) return true;
        var cleanKey = permissionKey.TrimStart('/');
        return Permissions.Any(p => p.Equals(cleanKey, StringComparison.OrdinalIgnoreCase));
    }
}