using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Documents.Audit;

/// <summary>
/// Immutable audit log document recorded in MongoDB for compliance, governance, and traceability.
/// Implements ITenantEntity for multi-tenant isolation.
/// </summary>
public class AuditLogDocument : ITenantEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public string ActionType { get; set; } = string.Empty; // "Command" or "Query"
    public string ActionName { get; set; } = string.Empty; // e.g. "CreateDebtCommand", "GetAllDebtsQuery"
    public string? Payload { get; set; }
    public long ExecutionDurationMs { get; set; }
    public bool IsSuccess { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? IpAddress { get; set; }
    public Guid? ImpersonatedByAdminId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
