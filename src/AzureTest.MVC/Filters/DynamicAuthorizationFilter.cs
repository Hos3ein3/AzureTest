using System.Collections.Generic;
using System.Linq;
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

    public DynamicAuthorizationFilter(IUnitOfWork unitOfWork, IRedisService redisService)
    {
        _unitOfWork = unitOfWork;
        _redisService = redisService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
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

            // Cache Permissions
            var allPermissions = await _redisService.GetAsync<List<Permission>>("AllPermissions");
            if (allPermissions == null)
            {
                allPermissions = (await _unitOfWork.Permissions.GetAllPermissionsAsync()).ToList();
                await _redisService.SetAsync("AllPermissions", allPermissions, System.TimeSpan.FromHours(24));
            }

            var permission = allPermissions.FirstOrDefault(p => p.ControllerName == controllerName && p.ActionName == actionName);

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
                if (currentUser.Permissions.Contains($"{controllerName}/{actionName}"))
                {
                    return; // Access granted via Redis cached permissions
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
