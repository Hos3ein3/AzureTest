using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Audit;

namespace AzureTest.Persistence.Repositories.NoSql;

/// <summary>
/// No-op implementation of IAuditLogRepository used when MongoDB is not configured.
/// </summary>
public class NullAuditLogRepository : NullDocumentRepository<AuditLogDocument>, IAuditLogRepository
{
    public Task<IReadOnlyList<AuditLogDocument>> GetFilteredLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? actionType,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<AuditLogDocument>>(Array.Empty<AuditLogDocument>());
    }

    public Task<long> CountFilteredLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? actionType,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0L);
    }

    public Task<long> DeleteLogsAsync(
        Guid? id,
        Guid? userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0L);
    }

    public Task<long> PurgeAllLogsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0L);
    }
}
