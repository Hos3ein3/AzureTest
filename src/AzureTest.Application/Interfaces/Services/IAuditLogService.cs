using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents.Audit;

namespace AzureTest.Application.Interfaces.Services;

/// <summary>
/// Generic service for recording and retrieving immutable audit logs.
/// Enforces multi-tenant data access: Users view only their own logs, Admins can view all logs.
/// </summary>
public interface IAuditLogService
{
    Task LogCommandAsync<TCommand>(TCommand command, long durationMs, bool isSuccess, string? error = null, CancellationToken cancellationToken = default);
    Task LogQueryAsync<TQuery>(TQuery query, long durationMs, bool isSuccess, string? error = null, CancellationToken cancellationToken = default);
    Task LogAsync(string actionType, string actionName, object? payload, long durationMs, bool isSuccess, string? error = null, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AuditLogDocument> Logs, long TotalCount)> GetLogsAsync(
        bool isAdmin,
        Guid currentUserId,
        Guid currentTenantId,
        string? actionType = null,
        string? searchTerm = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
