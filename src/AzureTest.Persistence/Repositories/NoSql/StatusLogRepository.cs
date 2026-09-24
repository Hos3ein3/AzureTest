using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class StatusLogRepository : DocumentRepository<StatusLog>, IStatusLogRepository
{
    public StatusLogRepository(IMongoDatabase database)
        : base(database, "StatusLogs")
    {
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<StatusLog>.IndexKeys.Ascending(x => x.ResumeId).Descending(x => x.Date);
        var indexOptions = new CreateIndexOptions { Background = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<StatusLog>(indexKeys, indexOptions));
    }

    public async Task<IReadOnlyList<StatusLog>> GetByResumeIdAsync(Guid resumeId)
    {
        var filter = Builders<StatusLog>.Filter.Eq(x => x.ResumeId, resumeId);
        return await _collection.Find(filter)
            .SortByDescending(x => x.Date)
            .ToListAsync();
    }
}
