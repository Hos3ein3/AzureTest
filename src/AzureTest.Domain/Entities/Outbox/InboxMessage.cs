using System;

namespace AzureTest.Domain.Entities.Outbox;

public sealed class InboxMessage
{
    public Guid Id { get; set; }
    public string HandlerName { get; set; } = string.Empty;
    public DateTime ReceivedOnUtc { get; set; } = DateTime.UtcNow;
    public DateTime ProcessedOnUtc { get; set; } = DateTime.UtcNow;
}
