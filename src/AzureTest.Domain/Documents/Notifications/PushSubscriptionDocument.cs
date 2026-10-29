using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Notifications;

public sealed class PushSubscriptionDocument : ITenantEntity
{
    public static class Rules
    {
        public const int EndpointMaxLength = 1000;
        public const int KeyMaxLength = 250;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Endpoint { get; private set; } = string.Empty;
    public string P256DH { get; private set; } = string.Empty;
    public string Auth { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private PushSubscriptionDocument() { }

    public static PushSubscriptionDocument Create(
        Guid userId,
        Guid tenantId,
        string endpoint,
        string p256dh,
        string auth)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint cannot be empty.", nameof(endpoint));

        return new PushSubscriptionDocument
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TenantId = tenantId,
            Endpoint = endpoint,
            P256DH = p256dh ?? string.Empty,
            Auth = auth ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }
}
