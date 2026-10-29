using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Notifications;

public sealed record NotificationDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Message,
    string Type,
    string? ActionUrl,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt,
    string TimeAgo,
    Dictionary<string, string>? Metadata = null
);

public sealed record WebPushSubscriptionDto(
    string Endpoint,
    string P256DH,
    string Auth
);

public sealed record MarkNotificationReadDto(
    Guid? NotificationId = null
);
