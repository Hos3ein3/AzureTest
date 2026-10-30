using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace AzureTest.API.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier 
                     ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirstValue("sub")
                     ?? Context.User?.FindFirstValue("id");

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.ToLowerInvariant()}");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Explicitly joins the user-specific notification group.
    /// Useful for clients passing user identification explicitly upon reconnect or session startup.
    /// </summary>
    public async Task JoinUserGroup(string userId)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Trim().ToLowerInvariant()}");
        }
    }
}
