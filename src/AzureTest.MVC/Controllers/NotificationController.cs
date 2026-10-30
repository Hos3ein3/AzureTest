using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Notifications;
using AzureTest.Application.Notifications.Commands.MarkNotificationAsRead;
using AzureTest.Application.Notifications.Commands.SubscribeWebPush;
using AzureTest.Application.Notifications.Queries.GetUserNotifications;
using AzureTest.Contracts.DTOs.Notifications;
using AzureTest.MVC.Filters;
using AzureTest.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
[BypassDynamicAuthorization]
[DisplayName("Notification")]
[Description("Manage real-time notifications, notification history, and web push subscriptions.")]
public class NotificationController : Controller
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

    [HttpGet]
    [DisplayName("Notification Center")]
    [Description("View, filter, and inspect user notifications.")]
    public async Task<IActionResult> Index(
        [FromQuery] Guid? id,
        [FromQuery] string? search,
        [FromQuery] string? filter,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return RedirectToAction("Login", "Auth");

        var query = new GetUserNotificationsQuery(userId, null, 150);
        var result = await _queries.GetUserNotifications.HandleAsync(query, cancellationToken);
        var notifications = result.IsSuccess ? result.Value : new List<NotificationDto>();

        NotificationDto? selected = null;
        if (id.HasValue && id.Value != Guid.Empty)
        {
            selected = notifications.FirstOrDefault(n => n.Id == id.Value);
            if (selected != null && !selected.IsRead)
            {
                await _commands.MarkAsRead.HandleAsync(new MarkNotificationAsReadCommand(userId, id.Value), cancellationToken);
                selected = selected with { IsRead = true, ReadAt = DateTime.UtcNow };
                notifications = notifications.Select(n => n.Id == id.Value ? selected : n).ToList();
            }
        }

        selected ??= notifications.FirstOrDefault();

        var vm = new NotificationIndexViewModel
        {
            Notifications = notifications,
            SelectedNotification = selected,
            SelectedId = selected?.Id.ToString(),
            UnreadCount = notifications.Count(n => !n.IsRead),
            SearchTerm = search,
            ActiveFilter = filter ?? "all"
        };

        return View(vm);
    }

    [HttpGet]
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

    [HttpPost]
    [IgnoreAntiforgeryToken]
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

    [HttpPost]
    [IgnoreAntiforgeryToken]
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

    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Get VAPID Public Key")]
    [Description("Get public VAPID key to initialize browser push subscription.")]
    public IActionResult GetVapidPublicKey()
    {
        var key = _webPushService.GetVapidPublicKey();
        return Ok(new { publicKey = key });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Notification")]
    [Description("Delete a single notification owned by current user.")]
    public async Task<IActionResult> Delete(DeleteNotificationRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        var success = await _notificationHistory.DeleteByIdForUserAsync(request.Id, userId, cancellationToken);
        
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                     Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjax)
        {
            return Ok(new { success });
        }

        if (success)
            TempData["Success"] = "Notification deleted successfully.";
        else
            TempData["Error"] = "Notification could not be deleted.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Delete Notifications by Date Range")]
    [Description("Delete notifications within a date range for current user.")]
    public async Task<IActionResult> DeleteByDateRange(DeleteNotificationsByDateRangeRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.Id;
        if (userId == Guid.Empty)
            return Unauthorized(new { success = false, error = "User is not authenticated." });

        DateTime? fromUtc = request.FromDate.HasValue ? DateTime.SpecifyKind(request.FromDate.Value.Date, DateTimeKind.Utc) : null;
        DateTime? toUtc = request.ToDate.HasValue ? DateTime.SpecifyKind(request.ToDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null;

        var deleted = await _notificationHistory.DeleteByUserAndDateRangeAsync(userId, fromUtc, toUtc, cancellationToken);

        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                     Request.Headers["Accept"].ToString().Contains("application/json");

        if (isAjax)
        {
            return Ok(new { success = true, deletedCount = deleted });
        }

        TempData["Success"] = $"{deleted} notification(s) deleted within specified date range.";
        return RedirectToAction(nameof(Index));
    }
}
