

using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class ResumeFilesRepository : DocumentRepository<ResumeFiles>, IResumeFilesRepository
{
    public ResumeFilesRepository(IMongoDatabase database)
        : base(database, "ResumeFiles")
    {
        // Ensure a unique index on ResumeId for fast lookups and data integrity
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<ResumeFiles>.IndexKeys.Ascending(x => x.ResumeId);
        var indexOptions = new CreateIndexOptions { Unique = true, Background = true };
        var indexModel = new CreateIndexModel<ResumeFiles>(indexKeys, indexOptions);
        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task<ResumeFiles?> GetByResumeIdAsync(Guid resumeId)
    {
        var filter = Builders<ResumeFiles>.Filter.Eq(x => x.ResumeId, resumeId);
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }
}
