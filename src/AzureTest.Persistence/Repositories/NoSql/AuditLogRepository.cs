using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Audit;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class AuditLogRepository : DocumentRepository<AuditLogDocument>, IAuditLogRepository
{
    public AuditLogRepository(IMongoDatabase database)
        : base(database, "AuditLogs")
    {
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<AuditLogDocument>.IndexKeys
            .Descending(x => x.Timestamp)
            .Ascending(x => x.TenantId)
            .Ascending(x => x.UserId)
            .Ascending(x => x.ActionType);
        _collection.Indexes.CreateOne(new CreateIndexModel<AuditLogDocument>(indexKeys, new CreateIndexOptions { Background = true }));
    }

    public async Task<IReadOnlyList<AuditLogDocument>> GetFilteredLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? actionType,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(tenantId, userId, actionType, searchTerm);
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 5, 200);

        return await _collection.Find(filter)
            .SortByDescending(x => x.Timestamp)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> CountFilteredLogsAsync(
        Guid? tenantId,
        Guid? userId,
        string? actionType,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        var filter = BuildFilter(tenantId, userId, actionType, searchTerm);
        return await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<long> DeleteLogsAsync(
        Guid? id,
        Guid? userId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken = default)
    {
        var builder = Builders<AuditLogDocument>.Filter;
        var filter = builder.Empty;

        if (id.HasValue && id.Value != Guid.Empty)
        {
            var idFilter = builder.Or(
                builder.Eq(x => x.Id, id.Value),
                builder.Eq("_id", id.Value),
                builder.Eq("Id", id.Value),
                builder.Eq("_id", id.Value.ToString()),
                builder.Eq("Id", id.Value.ToString())
            );
            filter &= idFilter;
        }

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            var userFilter = builder.Or(
                builder.Eq(x => x.UserId, userId.Value),
                builder.Eq("UserId", userId.Value),
                builder.Eq("UserId", userId.Value.ToString())
            );
            filter &= userFilter;
        }

        if (fromUtc.HasValue)
        {
            filter &= builder.Gte(x => x.Timestamp, fromUtc.Value.ToUniversalTime());
        }

        if (toUtc.HasValue)
        {
            filter &= builder.Lte(x => x.Timestamp, toUtc.Value.ToUniversalTime());
        }

        var result = await _collection.DeleteManyAsync(filter, cancellationToken);
        return result.DeletedCount;
    }

    public async Task<long> PurgeAllLogsAsync(CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteManyAsync(Builders<AuditLogDocument>.Filter.Empty, cancellationToken);
        return result.DeletedCount;
    }

    private static FilterDefinition<AuditLogDocument> BuildFilter(
        Guid? tenantId,
        Guid? userId,
        string? actionType,
        string? searchTerm)
    {
        var builder = Builders<AuditLogDocument>.Filter;
        var filter = builder.Empty;

        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            filter &= builder.Or(
                builder.Eq(x => x.TenantId, tenantId.Value),
                builder.Eq("TenantId", tenantId.Value),
                builder.Eq("TenantId", tenantId.Value.ToString())
            );
        }

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            filter &= builder.Or(
                builder.Eq(x => x.UserId, userId.Value),
                builder.Eq("UserId", userId.Value),
                builder.Eq("UserId", userId.Value.ToString())
            );
        }

        if (!string.IsNullOrWhiteSpace(actionType))
        {
            filter &= builder.Eq(x => x.ActionType, actionType.Trim());
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            var regex = new MongoDB.Bson.BsonRegularExpression(term, "i");
            var searchFilter = builder.Regex(x => x.ActionName, regex) |
                               builder.Regex(x => x.UserName, regex) |
                               builder.Regex(x => x.UserEmail, regex) |
                               builder.Regex(x => x.ErrorMessage, regex);
            filter &= searchFilter;
        }

        return filter;
    }
}
