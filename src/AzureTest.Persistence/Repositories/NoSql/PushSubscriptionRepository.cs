using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Notifications;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class PushSubscriptionRepository : DocumentRepository<PushSubscriptionDocument>, IPushSubscriptionRepository
{
    public PushSubscriptionRepository(IMongoDatabase database) : base(database, "PushSubscriptions")
    {
    }

    public async Task<IReadOnlyList<PushSubscriptionDocument>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<PushSubscriptionDocument>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter).ToListAsync(ct);
    }

    public async Task<bool> DeleteByEndpointAsync(Guid userId, string endpoint, CancellationToken ct = default)
    {
        var filter = Builders<PushSubscriptionDocument>.Filter.And(
            Builders<PushSubscriptionDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<PushSubscriptionDocument>.Filter.Eq(x => x.Endpoint, endpoint)
        );

        var result = await _collection.DeleteManyAsync(filter, ct);
        return result.DeletedCount > 0;
    }
}
