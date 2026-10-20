using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Media;
using AzureTest.Domain.Media.Enums;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class WatchItemRepository : DocumentRepository<WatchItemDocument>, IWatchItemRepository
{
    public WatchItemRepository(IMongoDatabase database) : base(database, "WatchItems")
    {
    }

    public async Task<WatchItemDocument?> GetByUserAndTmdbAsync(Guid userId, int tmdbId, MediaType type, CancellationToken ct = default)
    {
        var filter = Builders<WatchItemDocument>.Filter.And(
            Builders<WatchItemDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<WatchItemDocument>.Filter.Eq(x => x.TmdbId, tmdbId),
            Builders<WatchItemDocument>.Filter.Eq(x => x.Type, type)
        );

        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<WatchItemDocument>> GetUserWatchListAsync(Guid userId, WatchStatus? status = null, MediaType? type = null, CancellationToken ct = default)
    {
        var filterBuilder = Builders<WatchItemDocument>.Filter;
        var filter = filterBuilder.Eq(x => x.UserId, userId);

        if (status.HasValue)
        {
            filter &= filterBuilder.Eq(x => x.Status, status.Value);
        }

        if (type.HasValue)
        {
            filter &= filterBuilder.Eq(x => x.Type, type.Value);
        }

        return await _collection.Find(filter)
            .SortByDescending(x => x.LastWatchedAt)
            .ToListAsync(ct);
    }
}
