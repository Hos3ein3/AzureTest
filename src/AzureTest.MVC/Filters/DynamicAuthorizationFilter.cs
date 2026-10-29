using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.MVC.Filters;

public class DynamicAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRedisService _redisService;
    private readonly AzureTest.Infrastructure.Configuration.InfrastructureFeatureOptions _featureOptions;

    public DynamicAuthorizationFilter(
        IUnitOfWork unitOfWork, 
        IRedisService redisService,
        Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.InfrastructureFeatureOptions> featureOptions)
    {
        _unitOfWork = unitOfWork;
        _redisService = redisService;
        _featureOptions = featureOptions?.Value ?? new AzureTest.Infrastructure.Configuration.InfrastructureFeatureOptions();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any() ||
            context.ActionDescriptor.EndpointMetadata.OfType<BypassDynamicAuthorizationAttribute>().Any())
        {
            return;
        }

        var user = context.HttpContext.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            return; 
        }

        if (user.IsInRole("Admin"))
        {
            return;
        }
        
        if (context.ActionDescriptor is ControllerActionDescriptor tempDescriptor)
        {
            if (user.IsInRole("User"))
            {
                var feature=context.HttpContext.RequestServices
                    .GetService<Microsoft.Extensions.Options.IOptions<AzureTest.Infrastructure.Configuration.InfrastructureFeatureOptions>>();
                if (feature!.Value.EnableUserAccessToDashboardSlugs)
                {
                    var options = context.HttpContext.RequestServices
                        .GetService<Microsoft.Extensions.Options.IOptions<
                            AzureTest.Infrastructure.Configuration.DashboardSettings>>();
                    var reserved = options?.Value.ReservedSlugs ?? new List<string>();

                    if (reserved.Contains(tempDescriptor.ControllerName, System.StringComparer.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }
        }

        if (context.ActionDescriptor is ControllerActionDescriptor descriptor)
        {
            var controllerName = descriptor.ControllerName;
            var actionName = descriptor.ActionName;
            var areaName = context.RouteData.Values.TryGetValue("area", out var areaVal) ? areaVal?.ToString() : null;
            if (string.IsNullOrWhiteSpace(areaName))
            {
                var areaAttr = descriptor.ControllerTypeInfo.GetCustomAttribute<AreaAttribute>();
                areaName = areaAttr?.RouteValue;
            }

            // Cache Permissions
            var allPermissions = await _redisService.GetAsync<List<Permission>>("AllPermissions");
            if (allPermissions == null)
            {
                allPermissions = (await _unitOfWork.Permissions.GetAllPermissionsAsync()).ToList();
                await _redisService.SetAsync("AllPermissions", allPermissions, System.TimeSpan.FromHours(24));
            }

            // Match permission prioritizing AreaName if specified
            var permission = allPermissions.FirstOrDefault(p => 
                (string.IsNullOrWhiteSpace(areaName) ? string.IsNullOrWhiteSpace(p.AreaName) : string.Equals(p.AreaName, areaName, System.StringComparison.OrdinalIgnoreCase)) &&
                string.Equals(p.ControllerName, controllerName, System.StringComparison.OrdinalIgnoreCase) && 
                string.Equals(p.ActionName, actionName, System.StringComparison.OrdinalIgnoreCase));

            // Fallback match without area check if not found
            permission ??= allPermissions.FirstOrDefault(p => 
                string.Equals(p.ControllerName, controllerName, System.StringComparison.OrdinalIgnoreCase) && 
                string.Equals(p.ActionName, actionName, System.StringComparison.OrdinalIgnoreCase));

            // Everything is restricted by default. Allow only if explicitly marked as unrestricted.
            if (permission != null && !permission.IsRestricted)
            {
                return;
            }

            // At this point, the endpoint is restricted.
            // Check if CurrentUser (cached in Redis) has this permission string
            var currentUser = context.HttpContext.RequestServices.GetService<ICurrentUser>();
            if (currentUser != null && currentUser.Permissions != null)
            {
                var fullKeyWithArea = !string.IsNullOrWhiteSpace(areaName) ? $"{areaName}/{controllerName}/{actionName}" : null;
                var shortKey = $"{controllerName}/{actionName}";

                if ((fullKeyWithArea != null && currentUser.Permissions.Contains(fullKeyWithArea, System.StringComparer.OrdinalIgnoreCase)) ||
                    currentUser.Permissions.Contains(shortKey, System.StringComparer.OrdinalIgnoreCase))
                {
                    return; // Access granted via Redis cached permissions
                }
            }

            // Check Direct User Permissions Override (Feature Flag: EnableDirectUserPermissions)
            // If enabled, check if the user has been explicitly granted this permission.
            // If granted, bypass role requirements and allow access. If feature is disabled, bypass this check.
            if (_featureOptions.EnableDirectUserPermissions && permission != null)
            {
                var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (System.Guid.TryParse(userIdClaim, out var userId) && userId != System.Guid.Empty)
                {
                    var userPermCacheKey = $"UserPermissions_{userId}";
                    var userPermsIds = await _redisService.GetAsync<List<System.Guid>>(userPermCacheKey);
                    if (userPermsIds == null)
                    {
                        var userPerms = await _unitOfWork.UserPermissions.GetPermissionsByUserIdAsync(userId);
                        userPermsIds = userPerms.Where(up => up.IsGranted).Select(up => up.PermissionId).ToList();
                        await _redisService.SetAsync(userPermCacheKey, userPermsIds, System.TimeSpan.FromHours(24));
                    }

                    if (userPermsIds.Contains(permission.Id))
                    {
                        return; // Direct user permission granted - bypasses role requirements!
                    }
                }
            }

            // Fallback: check RolePermissions from DB/Redis
            var userRoles = user.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!userRoles.Any())
            {
                context.Result = new ForbidResult();
                return;
            }
            
            // Check cache for role permissions
            bool hasAccess = false;
            foreach(var role in userRoles)
            {
                var cacheKey = $"RolePermissions_{role}";
                var rolePermsIds = await _redisService.GetAsync<List<System.Guid>>(cacheKey);
                if (rolePermsIds == null)
                {
                    var rolePerms = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(role);
                    rolePermsIds = rolePerms.Select(rp => rp.PermissionId).ToList();
                    await _redisService.SetAsync(cacheKey, rolePermsIds, System.TimeSpan.FromHours(24));
                }
                
                if (permission != null && rolePermsIds.Contains(permission.Id))
                {
                    hasAccess = true;
                    break;
                }
            }

            if (!hasAccess)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
