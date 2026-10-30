using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Notifications;
using AzureTest.Application.Notifications.Commands.SubscribeWebPush;
using AzureTest.Contracts.DTOs.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Notification;

public static class SubscribeWebPush
{
    public static async Task<IResult> HandleAsync(
        [FromBody] WebPushSubscriptionDto? dto,
        [FromServices] INotificationCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        if (dto == null || string.IsNullOrWhiteSpace(dto.Endpoint))
            return Results.BadRequest(new { success = false, error = "Invalid subscription data." });

        var command = new SubscribeWebPushCommand(userId, currentUser.TenantId, dto.Endpoint, dto.P256DH, dto.Auth);
        var result = await commands.SubscribePush.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return Results.BadRequest(new { success = false, error = result.Error });

        return Results.Ok(new { success = true });
    }
}
