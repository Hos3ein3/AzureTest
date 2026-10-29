using System;
using System.Collections.Generic;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Notifications.Queries.GetUserNotifications;
using AzureTest.Contracts.DTOs.Notifications;

namespace AzureTest.Application.Notifications;

public class NotificationQueryUnit : INotificationQueryUnit
{
    public IQueryHandler<GetUserNotificationsQuery, Result<IReadOnlyList<NotificationDto>>> GetUserNotifications { get; }

    public NotificationQueryUnit(
        IQueryHandler<GetUserNotificationsQuery, Result<IReadOnlyList<NotificationDto>>> getUserNotifications)
    {
        GetUserNotifications = getUserNotifications ?? throw new ArgumentNullException(nameof(getUserNotifications));
    }
}
