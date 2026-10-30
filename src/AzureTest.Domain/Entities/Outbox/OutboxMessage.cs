using System;

using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.Outbox;

public sealed class OutboxMessage:BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
    public int RetryCount { get; set; } = 0;
}
