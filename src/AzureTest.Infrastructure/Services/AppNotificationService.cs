using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Notifications;
using AzureTest.Domain.Documents.Notifications;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

public class AppNotificationService : IAppNotificationService
{
    private readonly INoSqlUnitOfWork _noSqlUow;
    private readonly IWebPushService _webPushService;
    private readonly INotificationHubDispatcher _hubDispatcher;
    private readonly ILogger<AppNotificationService> _logger;

    public AppNotificationService(
        INoSqlUnitOfWork noSqlUow,
        IWebPushService webPushService,
        INotificationHubDispatcher hubDispatcher,
        ILogger<AppNotificationService> logger)
    {
        _noSqlUow = noSqlUow ?? throw new ArgumentNullException(nameof(noSqlUow));
        _webPushService = webPushService ?? throw new ArgumentNullException(nameof(webPushService));
        _hubDispatcher = hubDispatcher ?? throw new ArgumentNullException(nameof(hubDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Guid>> SendNotificationAsync(
        Guid userId,
        Guid tenantId,
        string title,
        string message,
        string type = "Trade",
        string? actionUrl = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Result<Guid>.Failure("UserId cannot be empty.");

        try
        {
            var doc = NotificationHistoryDocument.Create(
                userId,
                tenantId,
                title,
                message,
                type,
                actionUrl,
                metadata);

            // 1. Persist notification in MongoDB
            await _noSqlUow.NotificationHistories.AddAsync(doc, ct);

            var dto = new NotificationDto(
                doc.Id,
                doc.UserId,
                doc.Title,
                doc.Message,
                doc.Type,
                doc.ActionUrl,
                doc.IsRead,
                doc.ReadAt,
                doc.CreatedAt,
                "Just now",
                doc.Metadata);

            // 2. Broadcast via SignalR to user's connected clients
            try
            {
                await _hubDispatcher.SendToUserAsync(userId, dto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send SignalR notification to user {UserId}", userId);
            }

            // 3. Dispatch WebPush to registered browser endpoints
            try
            {
                var pushUrl = !string.IsNullOrWhiteSpace(actionUrl) ? actionUrl : $"/Notification?id={doc.Id}";
                await _webPushService.SendPushAsync(userId, title, message, pushUrl, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to dispatch WebPush notification to user {UserId}", userId);
            }

            return Result<Guid>.Success(doc.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create and dispatch notification for user {UserId}", userId);
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
