using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Application.Auth.Services;

public class AuthSessionService : IAuthSessionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRedisService _redisService;

    public AuthSessionService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IRedisService redisService)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _redisService = redisService ?? throw new ArgumentNullException(nameof(redisService));
    }

    public async Task CacheUserSessionAsync(ApplicationUser user, CancellationToken ct = default)
    {
        if (user == null) return;

        var roles = await _userManager.GetRolesAsync(user);

        // 1. Role Permissions
        var rolePermissions = new List<string>();
        foreach (var role in roles)
        {
            var rolePerms = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(role);
            foreach (var rp in rolePerms)
            {
                if (rp.Permission != null)
                {
                    var areaPrefix = !string.IsNullOrWhiteSpace(rp.Permission.AreaName) ? $"{rp.Permission.AreaName}/" : "";
                    rolePermissions.Add($"{areaPrefix}{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
                    rolePermissions.Add($"{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
                }
            }
        }

        // 2. Direct User Permissions
        var directPermissions = new List<string>();
        var userPerms = await _unitOfWork.UserPermissions.GetPermissionsByUserIdAsync(user.Id, ct);
        foreach (var up in userPerms.Where(x => x.IsGranted))
        {
            if (up.Permission != null)
            {
                var areaPrefix = !string.IsNullOrWhiteSpace(up.Permission.AreaName) ? $"{up.Permission.AreaName}/" : "";
                directPermissions.Add($"{areaPrefix}{up.Permission.ControllerName}/{up.Permission.ActionName}");
                directPermissions.Add($"{up.Permission.ControllerName}/{up.Permission.ActionName}");
            }
        }

        var distinctRolePerms = rolePermissions.Distinct().ToList();
        var distinctDirectPerms = directPermissions.Distinct().ToList();
        var combinedPermissions = distinctRolePerms.Concat(distinctDirectPerms).Distinct().ToList();

        var sessionDto = new UserSessionCacheDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            ProfileImage = user.ProfileImage,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = roles.ToList(),
            RolePermissions = distinctRolePerms,
            DirectPermissions = distinctDirectPerms,
            Permissions = combinedPermissions,
            IsReadOnly = user.IsReadOnly
        };

        await _redisService.SetAsync($"User_Info_{user.Id}", sessionDto, TimeSpan.FromDays(7));
    }

    public async Task InvalidateUserSessionAsync(Guid userId, CancellationToken ct = default)
    {
        if (userId == Guid.Empty) return;

        try
        {
            await _redisService.RemoveAsync($"User_Info_{userId}");
            await _redisService.RemoveAsync($"UserPermissions_{userId}");
        }
        catch
        {
            // Cache removal errors should not abort authentication operations
        }
    }
}
