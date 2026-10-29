using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Notifications;

public sealed class NotificationHistoryDocument : ITenantEntity
{
    public static class Rules
    {
        public const int TitleMaxLength = 200;
        public const int TitleMinLength = 1;
        public const int MessageMaxLength = 2000;
        public const int TypeMaxLength = 50;
    }

    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid UserId { get; private set; }
    public Guid TenantId { get; set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Type { get; private set; } = "Trade"; // "Trade", "System", "Security", "Ticket", etc.
    public string? ActionUrl { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Dictionary<string, string> Metadata { get; private set; } = new();

    private NotificationHistoryDocument() { }

    public static NotificationHistoryDocument Create(
        Guid userId,
        Guid tenantId,
        string title,
        string message,
        string type = "Trade",
        string? actionUrl = null,
        Dictionary<string, string>? metadata = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (title.Length > Rules.TitleMaxLength)
            title = title[..Rules.TitleMaxLength];

        if (message != null && message.Length > Rules.MessageMaxLength)
            message = message[..Rules.MessageMaxLength];

        return new NotificationHistoryDocument
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TenantId = tenantId,
            Title = title,
            Message = message ?? string.Empty,
            Type = string.IsNullOrWhiteSpace(type) ? "Trade" : type,
            ActionUrl = actionUrl,
            IsRead = false,
            ReadAt = null,
            CreatedAt = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, string>()
        };
    }

    public void MarkAsRead()
    {
        if (!IsRead)
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }

    public void MarkAsUnread()
    {
        if (IsRead)
        {
            IsRead = false;
            ReadAt = null;
        }
    }
}
