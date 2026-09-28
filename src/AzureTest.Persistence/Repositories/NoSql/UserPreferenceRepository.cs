using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly IMongoCollection<UserPreference> _collection;

    public UserPreferenceRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<UserPreference>("UserPreferences");
    }

    public async Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _collection.Find(x => x.UserId == userId).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(UserPreference userPreference, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(userPreference, new InsertOneOptions(), cancellationToken);
    }

    public async Task UpdateAsync(UserPreference userPreference, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(x => x.Id == userPreference.Id, userPreference, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }
}
