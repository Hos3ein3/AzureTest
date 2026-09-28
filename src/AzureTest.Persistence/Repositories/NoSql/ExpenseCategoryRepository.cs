using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;
using System;

namespace AzureTest.Persistence.Repositories.NoSql;

public class ExpenseCategoryRepository : DocumentRepository<ExpenseCategory>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(IMongoDatabase database)
        : base(database, "ExpenseCategories")
    {
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        var indexKeys = Builders<ExpenseCategory>.IndexKeys.Ascending(x => x.Name).Ascending(x => x.UserId);
        var indexOptions = new CreateIndexOptions { Background = true };
        _collection.Indexes.CreateOne(new CreateIndexModel<ExpenseCategory>(indexKeys, indexOptions));
    }

    public async Task<ExpenseCategory?> GetByNameAsync(string name, Guid userId)
    {
        var filter = Builders<ExpenseCategory>.Filter.And(
            Builders<ExpenseCategory>.Filter.Regex(
                x => x.Name,
                new MongoDB.Bson.BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(name)}$", "i")),
            Builders<ExpenseCategory>.Filter.Eq(x => x.UserId, userId)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ExpenseCategory>> SearchByNameAsync(string term, Guid userId)
    {
        var filter = Builders<ExpenseCategory>.Filter.And(
            Builders<ExpenseCategory>.Filter.Regex(
                x => x.Name,
                new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(term), "i")),
            Builders<ExpenseCategory>.Filter.Eq(x => x.UserId, userId)
        );
        return await _collection.Find(filter).SortBy(x => x.Name).ToListAsync();
    }
}
