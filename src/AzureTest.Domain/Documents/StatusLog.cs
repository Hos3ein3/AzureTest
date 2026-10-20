

using System;
using AzureTest.Domain.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents;

public class ResumeStatusLog : ITenantEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid ResumeId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ResumeStatus OldStatus { get; set; }
    public ResumeStatus NewStatus { get; set; }
    public string? Note { get; set; }
}
