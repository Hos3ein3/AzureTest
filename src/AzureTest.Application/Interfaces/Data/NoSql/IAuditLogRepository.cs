using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Documents.Audit;

namespace AzureTest.Application.Interfaces.Data.NoSql;

public interface IAuditLogRepository : IDocumentRepository<AuditLogDocument>
{
    Task<IReadOnlyList<AuditLogDocument>> GetFilteredLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? actionType,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    Task<long> CountFilteredLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? actionType,
        string? searchTerm,
        CancellationToken cancellationToken = default);

    Task<long> DeleteLogsAsync(
        Guid? id,
        Guid? userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default);

    Task<long> PurgeAllLogsAsync(CancellationToken cancellationToken = default);
}
