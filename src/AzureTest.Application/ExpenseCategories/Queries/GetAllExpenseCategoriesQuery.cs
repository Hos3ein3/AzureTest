using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.ExpenseCategories.Queries;

public record GetAllExpenseCategoriesQuery(Guid UserId) : IQuery<List<ExpenseCategory>>;

public class GetAllExpenseCategoriesQueryHandler : IQueryHandler<GetAllExpenseCategoriesQuery, List<ExpenseCategory>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public GetAllExpenseCategoriesQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<List<ExpenseCategory>> HandleAsync(GetAllExpenseCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"ExpCats_{request.UserId}";
        var cached = await _redisService.GetAsync<List<ExpenseCategory>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var all = await _noSqlUnitOfWork.ExpenseCategories.GetAllAsync();
        var userCategories = all.Where(c => c.UserId == request.UserId).ToList();

        await _redisService.SetAsync(cacheKey, userCategories, TimeSpan.FromHours(1));

        return userCategories;
    }
}
