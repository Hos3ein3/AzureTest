using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class JobSearchHistory : ITenantEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string DatePosted { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime SearchDate { get; set; } = DateTime.UtcNow;
}
