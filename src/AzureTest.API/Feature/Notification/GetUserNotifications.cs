using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Notifications;
using AzureTest.Application.Notifications.Queries.GetUserNotifications;
using AzureTest.Contracts.DTOs.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Notification;

public static class GetUserNotifications
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] GetUserNotificationsRequestDto request,
        [FromServices] INotificationQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var query = new GetUserNotificationsQuery(userId, request.UnreadOnly, request.Limit);
        var result = await queries.GetUserNotifications.HandleAsync(query, cancellationToken);

        if (!result.IsSuccess)
            return Results.BadRequest(new { success = false, error = result.Error });

        var notifications = result.Value;
        var unreadCount = notifications.Count(n => !n.IsRead);

        return Results.Ok(new
        {
            success = true,
            notifications,
            unreadCount
        });
    }
}
