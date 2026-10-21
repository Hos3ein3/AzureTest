

using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Domain.Documents;
using MongoDB.Driver;
using System;

namespace AzureTest.Persistence.Repositories.NoSql;

public class ExpenseCategoryRepository : DocumentRepository<ExpenseCategory>, IExpenseCategoryRepository
{
    public ExpenseCategoryRepository(IMongoDatabase database, AzureTest.Application.Interfaces.Services.ITenantProvider? tenantProvider = null)
        : base(database, "ExpenseCategories", tenantProvider)
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
        var filterBuilder = Builders<ExpenseCategory>.Filter;
        var nameFilter = filterBuilder.Regex(
            x => x.Name,
            new MongoDB.Bson.BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(name)}$", "i"));

        FilterDefinition<ExpenseCategory> filter;
        if (_tenantProvider?.IsMultiTenancyEnabled == true && _tenantProvider.CurrentTenantId != Guid.Empty)
        {
            var tenantFilter = filterBuilder.Or(
                filterBuilder.Eq(x => x.TenantId, _tenantProvider.CurrentTenantId),
                filterBuilder.Eq(x => x.UserId, _tenantProvider.CurrentTenantId),
                filterBuilder.Eq(x => x.UserId, userId)
            );
            filter = filterBuilder.And(nameFilter, tenantFilter);
        }
        else
        {
            filter = filterBuilder.And(nameFilter, filterBuilder.Eq(x => x.UserId, userId));
        }

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ExpenseCategory>> SearchByNameAsync(string term, Guid userId)
    {
        var filterBuilder = Builders<ExpenseCategory>.Filter;
        var termFilter = filterBuilder.Regex(
            x => x.Name,
            new MongoDB.Bson.BsonRegularExpression(System.Text.RegularExpressions.Regex.Escape(term), "i"));

        FilterDefinition<ExpenseCategory> filter;
        if (_tenantProvider?.IsMultiTenancyEnabled == true && _tenantProvider.CurrentTenantId != Guid.Empty)
        {
            var tenantFilter = filterBuilder.Or(
                filterBuilder.Eq(x => x.TenantId, _tenantProvider.CurrentTenantId),
                filterBuilder.Eq(x => x.UserId, _tenantProvider.CurrentTenantId),
                filterBuilder.Eq(x => x.UserId, userId)
            );
            filter = filterBuilder.And(termFilter, tenantFilter);
        }
        else
        {
            filter = filterBuilder.And(termFilter, filterBuilder.Eq(x => x.UserId, userId));
        }

        return await _collection.Find(filter).SortBy(x => x.Name).ToListAsync();
    }
}
