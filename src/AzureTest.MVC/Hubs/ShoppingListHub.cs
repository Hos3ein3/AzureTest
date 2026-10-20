using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AzureTest.MVC.Hubs;

public record ShoppingUserPresence(string ConnectionId, string UserId, string UserName, DateTime ConnectedAt);

[Authorize]
public class ShoppingListHub : Hub
{
    public const string GroupName = "ShoppingListShared";
    private static readonly ConcurrentDictionary<string, ShoppingUserPresence> _connectedUsers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier 
                     ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
                     ?? Context.ConnectionId;

        var userName = Context.User?.Identity?.Name 
                       ?? Context.User?.FindFirstValue(ClaimTypes.Email) 
                       ?? "Registered User";

        var presence = new ShoppingUserPresence(Context.ConnectionId, userId, userName, DateTime.UtcNow);
        _connectedUsers[Context.ConnectionId] = presence;

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName);
        await BroadcastActiveUsers();

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectedUsers.TryRemove(Context.ConnectionId, out _);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName);
        await BroadcastActiveUsers();

        await base.OnDisconnectedAsync(exception);
    }

    public async Task BroadcastActiveUsers()
    {
        var activeUsers = _connectedUsers.Values
            .GroupBy(u => u.UserId)
            .Select(g => g.First())
            .Select(u => new { userId = u.UserId, userName = u.UserName })
            .ToList();

        await Clients.Group(GroupName).SendAsync("ActiveUsersUpdated", activeUsers);
    }

    public async Task ItemStatusToggled(string itemId, string newStatus, string itemName)
    {
        var userName = Context.User?.Identity?.Name ?? "Registered User";
        await Clients.OthersInGroup(GroupName).SendAsync("ItemStatusChanged", new
        {
            itemId,
            status = newStatus,
            userName,
            itemName
        });
    }
}
