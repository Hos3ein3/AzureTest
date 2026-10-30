using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Notifications;
using Microsoft.AspNetCore.SignalR;

namespace AzureTest.API.Hubs;

public class NotificationHubDispatcher : INotificationHubDispatcher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationHubDispatcher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task SendToUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default)
    {
        var groupName = $"user_{userId.ToString().ToLowerInvariant()}";
        await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification, cancellationToken: ct);
    }

    public async Task SendToAllAsync(NotificationDto notification, CancellationToken ct = default)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification, cancellationToken: ct);
    }
}
