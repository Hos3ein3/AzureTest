using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Media;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class CustomMediaListRepository : DocumentRepository<CustomMediaListDocument>, ICustomMediaListRepository
{
    public CustomMediaListRepository(IMongoDatabase database) : base(database, "CustomMediaLists")
    {
    }

    public async Task<IReadOnlyList<CustomMediaListDocument>> GetUserListsAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<CustomMediaListDocument>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
