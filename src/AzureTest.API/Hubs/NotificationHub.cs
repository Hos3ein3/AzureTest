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
                     ?? Context.ConnectionId;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.ToLowerInvariant()}");
        }

        await base.OnConnectedAsync();
    }
}
