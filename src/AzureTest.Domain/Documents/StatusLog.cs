using System;
using AzureTest.Domain.Enums;

namespace AzureTest.Domain.Documents;

public class ResumeStatusLog
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid ResumeId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public ResumeStatus OldStatus { get; set; }
    public ResumeStatus NewStatus { get; set; }
    public string? Note { get; set; }
}
