using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Notification;

public static class DeleteNotificationsByDateRange
{
    public static async Task<IResult> HandleAsync(
        [FromBody] DeleteNotificationsByDateRangeRequestDto request,
        [FromServices] INotificationHistoryRepository notificationHistory,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        DateTime? fromUtc = request.FromDate.HasValue ? DateTime.SpecifyKind(request.FromDate.Value.Date, DateTimeKind.Utc) : null;
        DateTime? toUtc = request.ToDate.HasValue ? DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

        var deleted = await notificationHistory.DeleteByUserAndDateRangeAsync(userId, fromUtc, toUtc, cancellationToken);
        return Results.Ok(new { success = true, deletedCount = deleted });
    }
}
