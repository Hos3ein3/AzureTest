using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Notification;

public static class DeleteNotification
{
    public static async Task<IResult> HandleAsync(
        [FromBody] DeleteNotificationRequestDto request,
        [FromServices] INotificationHistoryRepository notificationHistory,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var success = await notificationHistory.DeleteByIdForUserAsync(request.Id, userId, cancellationToken);
        return Results.Ok(new { success });
    }
}
