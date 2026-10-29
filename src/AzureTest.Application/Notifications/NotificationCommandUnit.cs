using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Notifications.Commands.MarkNotificationAsRead;
using AzureTest.Application.Notifications.Commands.SubscribeWebPush;

namespace AzureTest.Application.Notifications;

public class NotificationCommandUnit : INotificationCommandUnit
{
    public ICommandHandler<MarkNotificationAsReadCommand, Result<int>> MarkAsRead { get; }
    public ICommandHandler<SubscribeWebPushCommand, Result> SubscribePush { get; }

    public NotificationCommandUnit(
        ICommandHandler<MarkNotificationAsReadCommand, Result<int>> markAsRead,
        ICommandHandler<SubscribeWebPushCommand, Result> subscribePush)
    {
        MarkAsRead = markAsRead ?? throw new ArgumentNullException(nameof(markAsRead));
        SubscribePush = subscribePush ?? throw new ArgumentNullException(nameof(subscribePush));
    }
}
