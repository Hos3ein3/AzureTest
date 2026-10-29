using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Notifications;

namespace AzureTest.Application.Interfaces.Services;

public interface INotificationHubDispatcher
{
    Task SendToUserAsync(Guid userId, NotificationDto notification, CancellationToken ct = default);
    Task SendToAllAsync(NotificationDto notification, CancellationToken ct = default);
}
