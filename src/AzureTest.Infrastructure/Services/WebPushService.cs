using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Notifications;
using AzureTest.Domain.Documents.Notifications;
using AzureTest.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebPush;

namespace AzureTest.Infrastructure.Services;

public class WebPushService : IWebPushService
{
    private readonly INoSqlUnitOfWork _noSqlUow;
    private readonly WebPushSettings _settings;
    private readonly ILogger<WebPushService> _logger;
    private readonly WebPushClient _client;
    private readonly VapidDetails _vapidDetails;

    public WebPushService(
        INoSqlUnitOfWork noSqlUow,
        IOptions<WebPushSettings> options,
        ILogger<WebPushService> logger)
    {
        _noSqlUow = noSqlUow ?? throw new ArgumentNullException(nameof(noSqlUow));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = options?.Value ?? new WebPushSettings();
        _client = new WebPushClient();

        // If keys are missing, generate dynamic fallback keys so dev environments work out of the box
        if (string.IsNullOrWhiteSpace(_settings.PublicKey) || string.IsNullOrWhiteSpace(_settings.PrivateKey))
        {
            var generated = VapidHelper.GenerateVapidKeys();
            _settings.PublicKey = generated.PublicKey;
            _settings.PrivateKey = generated.PrivateKey;
            _logger.LogInformation("WebPush VAPID keys were automatically initialized.");
        }

        _vapidDetails = new VapidDetails(_settings.Subject, _settings.PublicKey, _settings.PrivateKey);
    }

    public string GetVapidPublicKey() => _settings.PublicKey;

    public async Task<Result> SubscribeAsync(
        Guid userId,
        Guid tenantId,
        WebPushSubscriptionDto subscription,
        CancellationToken ct = default)
    {
        if (userId == Guid.Empty)
            return Result.Failure("UserId is required.");

        if (string.IsNullOrWhiteSpace(subscription.Endpoint))
            return Result.Failure("Push endpoint is required.");

        try
        {
            // Remove existing subscription with same endpoint for this user to avoid duplicates
            await _noSqlUow.PushSubscriptions.DeleteByEndpointAsync(userId, subscription.Endpoint, ct);

            var doc = PushSubscriptionDocument.Create(
                userId,
                tenantId,
                subscription.Endpoint,
                subscription.P256DH,
                subscription.Auth);

            await _noSqlUow.PushSubscriptions.AddAsync(doc, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register WebPush subscription for user {UserId}", userId);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UnsubscribeAsync(Guid userId, string endpoint, CancellationToken ct = default)
    {
        try
        {
            await _noSqlUow.PushSubscriptions.DeleteByEndpointAsync(userId, endpoint, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unsubscribe WebPush for user {UserId}", userId);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> SendPushAsync(
        Guid userId,
        string title,
        string message,
        string? actionUrl = null,
        CancellationToken ct = default)
    {
        try
        {
            var subscriptions = await _noSqlUow.PushSubscriptions.GetByUserIdAsync(userId, ct);
            if (subscriptions.Count == 0)
                return Result.Success();

            var payload = JsonSerializer.Serialize(new
            {
                title,
                body = message,
                icon = "/icon-192.png",
                badge = "/icon-192.png",
                data = new
                {
                    url = actionUrl ?? "/"
                }
            });

            foreach (var sub in subscriptions)
            {
                try
                {
                    var pushSub = new PushSubscription(sub.Endpoint, sub.P256DH, sub.Auth);
                    await _client.SendNotificationAsync(pushSub, payload, _vapidDetails, ct);
                }
                catch (WebPushException wpex) when (wpex.StatusCode == System.Net.HttpStatusCode.Gone || wpex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Subscription has expired or unsubscribed on browser side
                    await _noSqlUow.PushSubscriptions.DeleteByEndpointAsync(userId, sub.Endpoint, ct);
                    _logger.LogInformation("Removed expired WebPush subscription for user {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send WebPush notification to endpoint {Endpoint} for user {UserId}", sub.Endpoint, userId);
                }
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while dispatching WebPush notifications to user {UserId}", userId);
            return Result.Failure(ex.Message);
        }
    }
}
