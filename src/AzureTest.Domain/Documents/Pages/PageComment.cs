using System;

namespace AzureTest.Domain.Documents.Pages;

public class PageComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? AdminReply { get; set; }
    public DateTime? AdminReplyAt { get; set; }
}
