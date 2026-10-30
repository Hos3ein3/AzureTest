using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Notifications;

namespace AzureTest.Infrastructure.Services;

public class NullNotificationHubDispatcher : INotificationHubDispatcher
{
    public Task SendToUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendToAllAsync(NotificationDto notification, CancellationToken ct = default)
        => Task.CompletedTask;
}
