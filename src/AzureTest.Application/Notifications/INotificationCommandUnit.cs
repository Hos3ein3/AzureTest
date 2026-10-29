using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.Notifications.Commands.MarkNotificationAsRead;
using AzureTest.Application.Notifications.Commands.SubscribeWebPush;

namespace AzureTest.Application.Notifications;

public interface INotificationCommandUnit
{
    ICommandHandler<MarkNotificationAsReadCommand, Result<int>> MarkAsRead { get; }
    ICommandHandler<SubscribeWebPushCommand, Result> SubscribePush { get; }
}
