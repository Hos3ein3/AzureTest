using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Contracts.DTOs.Notifications;

namespace AzureTest.Application.Interfaces.Services;

public interface IWebPushService
{
    Task<Result> SendPushAsync(Guid userId, string title, string message, string? actionUrl = null, CancellationToken ct = default);
    Task<Result> SubscribeAsync(Guid userId, Guid tenantId, WebPushSubscriptionDto subscription, CancellationToken ct = default);
    Task<Result> UnsubscribeAsync(Guid userId, string endpoint, CancellationToken ct = default);
    string GetVapidPublicKey();
}
