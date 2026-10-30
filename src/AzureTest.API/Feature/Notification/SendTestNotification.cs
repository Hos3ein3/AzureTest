using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Notification;

public static class SendTestNotification
{
    public static async Task<IResult> HandleAsync(
        [FromServices] IAppNotificationService notificationService,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        await notificationService.SendNotificationAsync(
            userId,
            currentUser.TenantId,
            "Trade Alert: BTCUSDT Filled",
            $"Market Buy 0.05 BTC filled at 68,500 USDT ({DateTime.UtcNow:HH:mm:ss} UTC).",
            "Trade",
            "/TradingDashboard",
            null,
            cancellationToken);

        return Results.Ok(new { success = true, message = "Test notification sent successfully via SignalR and WebPush." });
    }
}
