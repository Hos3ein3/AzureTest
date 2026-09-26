using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class ResumeStatusLogRepository : DocumentRepository<ResumeStatusLog>, IResumeStatusLogRepository
{
    public ResumeStatusLogRepository(IMongoDatabase database)
        : base(database, "StatusLogs")
    {
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<ResumeStatusLog>.IndexKeys.Ascending(x => x.ResumeId).Descending(x => x.Date);
        var indexOptions = new CreateIndexOptions { Background = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<ResumeStatusLog>(indexKeys, indexOptions));
    }

    public async Task<IReadOnlyList<ResumeStatusLog>> GetByResumeIdAsync(Guid resumeId)
    {
        var filter = Builders<ResumeStatusLog>.Filter.Eq(x => x.ResumeId, resumeId);
        return await _collection.Find(filter)
            .SortByDescending(x => x.Date)
            .ToListAsync();
    }
}
