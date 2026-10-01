

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class UserDashboardRepository : DocumentRepository<UserDashboard>, IUserDashboardRepository
{
    public UserDashboardRepository(IMongoDatabase database)
        : base(database, "UserDashboards")
    {
        // Ensure a unique index on ResumeId for fast lookups and data integrity
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<UserDashboard>.IndexKeys.Ascending(x => x.Name);
        var indexOptions = new CreateIndexOptions { Unique = true, Background = true };
        var indexModel = new CreateIndexModel<UserDashboard>(indexKeys, indexOptions);
        _collection.Indexes.CreateOne(indexModel);
    }

    public async Task<IEnumerable<UserDashboard>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<UserDashboard?> GetByUserIdAndSlugAsync(Guid userId, string slug, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.UserId == userId && x.Slug == slug).FirstOrDefaultAsync(cancellationToken);
    }
}
