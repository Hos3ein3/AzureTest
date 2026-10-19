using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using AzureTest.MVC.Services;

namespace AzureTest.MVC.Hubs;

public class NotificationHub : Hub
{
    private readonly IUserTracker _userTracker;

    public NotificationHub(IUserTracker userTracker)
    {
        _userTracker = userTracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Context.ConnectionId;
        var userName = Context.User?.Identity?.Name 
                       ?? Context.User?.FindFirstValue(ClaimTypes.Email)
                       ?? (Context.User?.Identity?.IsAuthenticated == true ? "Authenticated User" : $"Guest ({Context.ConnectionId[..Math.Min(4, Context.ConnectionId.Length)]})");

        _userTracker.UserConnected(Context.ConnectionId, userId, userName);

        var onlineCount = _userTracker.GetOnlineUsersCount();
        await Clients.All.SendAsync("UpdateOnlineUsersCount", onlineCount);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _userTracker.UserDisconnected(Context.ConnectionId);

        var onlineCount = _userTracker.GetOnlineUsersCount();
        await Clients.All.SendAsync("UpdateOnlineUsersCount", onlineCount);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetOnlineUsersCount()
    {
        var onlineCount = _userTracker.GetOnlineUsersCount();
        await Clients.Caller.SendAsync("UpdateOnlineUsersCount", onlineCount);
    }
}
