using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Notifications;
using AzureTest.Application.Notifications.Commands.MarkNotificationAsRead;
using AzureTest.Application.Notifications.Commands.SubscribeWebPush;
using AzureTest.Application.Notifications.Queries.GetUserNotifications;
using AzureTest.Contracts.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[DisplayName("Notification")]
[Description("Manage real-time notifications, notification history, and web push subscriptions.")]
public class NotificationController : Controller
{
    private readonly INotificationCommandUnit _commands;
    private readonly INotificationQueryUnit _queries;
    private readonly IWebPushService _webPushService;
    private readonly ICurrentUser _currentUser;

    public NotificationController(
        INotificationCommandUnit commands,
        INotificationQueryUnit queries,
        IWebPushService webPushService,
        ICurrentUser currentUser)
    {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _webPushService = webPushService ?? throw new ArgumentNullException(nameof(webPushService));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    [HttpGet]
    [DisplayName("Get User Notifications")]
    [Description("Retrieve list of recent notifications for the authenticated user.")]
    public async Task<IActionResult> GetUserNotifications(
        [FromQuery] bool? unreadOnly = null,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = new GetUserNotificationsQuery(userId, unreadOnly, limit);
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

    [HttpPost]
    [DisplayName("Mark Notification As Read")]
    [Description("Mark a single notification or all notifications as read for current user.")]
    public async Task<IActionResult> MarkAsRead(
        [FromBody] MarkNotificationReadDto? dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized();

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

    [HttpPost]
    [DisplayName("Subscribe Web Push")]
    [Description("Register browser WebPush subscription for user.")]
    public async Task<IActionResult> SubscribePush(
        [FromBody] WebPushSubscriptionDto? dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized();

        if (dto == null || string.IsNullOrWhiteSpace(dto.Endpoint))
            return BadRequest(new { success = false, error = "Invalid subscription data." });

        var command = new SubscribeWebPushCommand(userId, _currentUser.TenantId, dto.Endpoint, dto.P256DH, dto.Auth);
        var result = await _commands.SubscribePush.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { success = false, error = result.Error });

        return Ok(new { success = true });
    }

    [HttpGet]
    [DisplayName("Get VAPID Public Key")]
    [Description("Get public VAPID key to initialize browser push subscription.")]
    public IActionResult GetVapidPublicKey()
    {
        var key = _webPushService.GetVapidPublicKey();
        return Ok(new { publicKey = key });
    }
}
