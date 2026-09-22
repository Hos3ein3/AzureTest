using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;

namespace AzureTest.Persistence.Repositories.NoSql;

public class CompanyRepository : DocumentRepository<Company>, ICompanyRepository
{
    public CompanyRepository(IMongoDatabase database)
        : base(database, "Companies")
    {
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<Company>.IndexKeys.Ascending(x => x.CompanyName);
        var indexOptions = new CreateIndexOptions { Background = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<Company>(indexKeys, indexOptions));
    }

    public async Task<Company?> GetByNameAsync(string companyName)
    {
        var filter = Builders<Company>.Filter.Regex(
            x => x.CompanyName,
            new MongoDB.Bson.BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(companyName)}$", "i"));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Company>> SearchByNameAsync(string term)
    {
        var filter = Builders<Company>.Filter.Regex(
            x => x.CompanyName,
            new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(term), "i"));
        return await _collection.Find(filter).SortBy(x => x.CompanyName).ToListAsync();
    }
}
