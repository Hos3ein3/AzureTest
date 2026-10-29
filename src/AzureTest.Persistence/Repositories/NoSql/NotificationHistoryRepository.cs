using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Notifications;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class NotificationHistoryRepository : DocumentRepository<NotificationHistoryDocument>, INotificationHistoryRepository
{
    public NotificationHistoryRepository(IMongoDatabase database) : base(database, "NotificationHistories")
    {
    }

    public async Task<IReadOnlyList<NotificationHistoryDocument>> GetByUserAsync(
        Guid userId,
        bool? unreadOnly = null,
        int limit = 50,
        CancellationToken ct = default)
    {
        var filterBuilder = Builders<NotificationHistoryDocument>.Filter;
        var filter = filterBuilder.Eq(x => x.UserId, userId);

        if (unreadOnly.HasValue && unreadOnly.Value)
        {
            filter &= filterBuilder.Eq(x => x.IsRead, false);
        }

        return await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .Limit(limit)
            .ToListAsync(ct);
    }

    public async Task<long> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<NotificationHistoryDocument>.Filter.And(
            Builders<NotificationHistoryDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<NotificationHistoryDocument>.Filter.Eq(x => x.IsRead, false)
        );

        return await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
    }

    public async Task<long> DeleteReadOlderThanAsync(DateTime cutoffUtc, CancellationToken ct = default)
    {
        var filter = Builders<NotificationHistoryDocument>.Filter.And(
            Builders<NotificationHistoryDocument>.Filter.Eq(x => x.IsRead, true),
            Builders<NotificationHistoryDocument>.Filter.Lte(x => x.ReadAt, cutoffUtc)
        );

        var result = await _collection.DeleteManyAsync(filter, ct);
        return result.DeletedCount;
    }
}
