using System.Collections.Generic;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Notifications.Queries.GetUserNotifications;
using AzureTest.Contracts.DTOs.Notifications;

namespace AzureTest.Application.Notifications;

public interface INotificationQueryUnit
{
    IQueryHandler<GetUserNotificationsQuery, Result<IReadOnlyList<NotificationDto>>> GetUserNotifications { get; }
}
