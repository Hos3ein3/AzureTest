

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class DebtPersonRepository : DocumentRepository<DebtPerson>, IDebtPersonRepository
{
    public DebtPersonRepository(IMongoDatabase database)
        : base(database, "DebtPersons")
    {
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<DebtPerson>.IndexKeys.Ascending(x => x.FullName);
        var indexOptions = new CreateIndexOptions { Background = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<DebtPerson>(indexKeys, indexOptions));
    }

    public async Task<IReadOnlyList<DebtPerson>> SearchByNameAsync(string term)
    {
        var filter = Builders<DebtPerson>.Filter.Regex(
            x => x.FullName,
            new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(term), "i"));
        return await _collection.Find(filter).SortBy(x => x.FullName).ToListAsync();
    }
}
