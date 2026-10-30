using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Notifications;
using AzureTest.Application.Notifications.Commands.MarkNotificationAsRead;
using AzureTest.Application.Notifications.Commands.SubscribeWebPush;
using AzureTest.Application.Notifications.Queries.GetUserNotifications;
using AzureTest.Contracts.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[DisplayName("Notification")]
[Description("Manage real-time notifications, notification history, and web push subscriptions.")]
public class NotificationController : ControllerBase
{
    private readonly INotificationCommandUnit _commands;
    private readonly INotificationQueryUnit _queries;
    private readonly IWebPushService _webPushService;
    private readonly IAppNotificationService _notificationService;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationHistoryRepository _notificationHistory;

    public NotificationController(
        INotificationCommandUnit commands,
        INotificationQueryUnit queries,
        IWebPushService webPushService,
        IAppNotificationService notificationService,
        ICurrentUser currentUser,
        INotificationHistoryRepository notificationHistory)
    {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _webPushService = webPushService ?? throw new ArgumentNullException(nameof(webPushService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _notificationHistory = notificationHistory ?? throw new ArgumentNullException(nameof(notificationHistory));
    }

    [HttpGet("user")]
    [DisplayName("Get User Notifications")]
    [Description("Retrieve list of recent notifications for the authenticated user.")]
    public async Task<IActionResult> GetUserNotifications(
        [FromQuery] GetUserNotificationsRequestDto? request,
        CancellationToken cancellationToken = default)
    {
        request ??= new GetUserNotificationsRequestDto();
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        var query = new GetUserNotificationsQuery(userId, request.UnreadOnly, request.Limit);
        var result = await _queries.GetUserNotifications.HandleAsync(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, error = result.Error });

        var notifications = result.Value;
        var unreadCount = notifications.Count(n => !n.IsRead);

        return Ok(new
        {
            success = true,
            notifications,
            unreadCount
        });
    }

    [HttpPost("mark-read")]
    [DisplayName("Mark Notification As Read")]
    [Description("Mark a single notification or all notifications as read for current user.")]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkNotificationReadDto? dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        var command = new MarkNotificationAsReadCommand(userId, dto?.NotificationId);
        var result = await _commands.MarkAsRead.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, error = result.Error });

        return Ok(new
        {
            success = true,
            markedCount = result.Value
        });
    }

    [HttpPost("subscribe-push")]
    [DisplayName("Subscribe Web Push")]
    [Description("Register browser WebPush subscription for user.")]
    public async Task<IActionResult> SubscribePush(
        [FromBody] WebPushSubscriptionDto? dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        if (dto == null || string.IsNullOrWhiteSpace(dto.Endpoint))
            return BadRequest(new { success = false, error = "Invalid subscription data." });

        var command = new SubscribeWebPushCommand(userId, _currentUser.TenantId, dto.Endpoint, dto.P256DH, dto.Auth);
        var result = await _commands.SubscribePush.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, error = result.Error });

        return Ok(new { success = true });
    }

    [HttpGet("vapid-public-key")]
    [AllowAnonymous]
    [DisplayName("Get VAPID Public Key")]
    [Description("Get public VAPID key to initialize browser push subscription.")]
    public IActionResult GetVapidPublicKey()
    {
        var key = _webPushService.GetVapidPublicKey();
        return Ok(new { publicKey = key });
    }

    [HttpPost("send-test")]
    [DisplayName("Send Test Notification")]
    [Description("Send a test notification to the current user via SignalR and WebPush.")]
    public async Task<IActionResult> SendTestNotification(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        await _notificationService.SendNotificationAsync(
            userId,
            _currentUser.TenantId,
            "Trade Alert: BTCUSDT Filled",
            $"Market Buy 0.05 BTC filled at 68,500 USDT ({DateTime.UtcNow:HH:mm:ss} UTC).",
            "Trade",
            "/TradingDashboard",
            null,
            cancellationToken);

        return Ok(new { success = true, message = "Test notification sent successfully via SignalR and WebPush." });
    }

    [HttpPost("delete")]
    [DisplayName("Delete Notification")]
    [Description("Delete a single notification owned by current user.")]
    public async Task<IActionResult> Delete([FromBody] DeleteNotificationRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        var success = await _notificationHistory.DeleteByIdForUserAsync(request.Id, userId, cancellationToken);
        return Ok(new { success });
    }

    [HttpPost("delete-by-date-range")]
    [DisplayName("Delete Notifications by Date Range")]
    [Description("Delete notifications within a date range for current user.")]
    public async Task<IActionResult> DeleteByDateRange([FromBody] DeleteNotificationsByDateRangeRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        DateTime? fromUtc = request.FromDate.HasValue ? DateTime.SpecifyKind(request.FromDate.Value.Date, DateTimeKind.Utc) : null;
        DateTime? toUtc = request.ToDate.HasValue ? DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

        var deleted = await _notificationHistory.DeleteByUserAndDateRangeAsync(userId, fromUtc, toUtc, cancellationToken);
        return Ok(new { success = true, deletedCount = deleted });
    }
}
