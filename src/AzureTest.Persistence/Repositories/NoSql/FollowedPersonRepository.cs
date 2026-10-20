using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents.Media;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class FollowedPersonRepository : DocumentRepository<FollowedPersonDocument>, IFollowedPersonRepository
{
    public FollowedPersonRepository(IMongoDatabase database) : base(database, "FollowedPeople")
    {
    }

    public async Task<FollowedPersonDocument?> GetByUserAndPersonAsync(Guid userId, int tmdbPersonId, CancellationToken ct = default)
    {
        var filter = Builders<FollowedPersonDocument>.Filter.And(
            Builders<FollowedPersonDocument>.Filter.Eq(x => x.UserId, userId),
            Builders<FollowedPersonDocument>.Filter.Eq(x => x.TmdbPersonId, tmdbPersonId)
        );

        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<FollowedPersonDocument>> GetUserFollowedPeopleAsync(Guid userId, CancellationToken ct = default)
    {
        var filter = Builders<FollowedPersonDocument>.Filter.Eq(x => x.UserId, userId);
        return await _collection.Find(filter)
            .SortByDescending(x => x.FollowedAt)
            .ToListAsync(ct);
    }
}
