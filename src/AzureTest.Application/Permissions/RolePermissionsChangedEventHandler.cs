using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Application.Messaging;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Application.Permissions;

public class RolePermissionsChangedEventHandler : IEventHandler<RolePermissionsChangedEvent>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RolePermissionsChangedEventHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task HandleAsync(RolePermissionsChangedEvent @event, CancellationToken cancellationToken = default)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(@event.RoleName);
        foreach (var user in usersInRole)
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }
    }
}
