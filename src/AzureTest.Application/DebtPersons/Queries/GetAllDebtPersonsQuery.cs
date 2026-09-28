using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.DebtPersons.Queries;

public record GetAllDebtPersonsQuery(Guid UserId) : IQuery<List<DebtPerson>>;

public class GetAllDebtPersonsQueryHandler : IQueryHandler<GetAllDebtPersonsQuery, List<DebtPerson>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public GetAllDebtPersonsQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<List<DebtPerson>> HandleAsync(GetAllDebtPersonsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"DebtPersons_{request.UserId}";
        var cached = await _redisService.GetAsync<List<DebtPerson>>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var all = await _noSqlUnitOfWork.DebtPersons.GetAllAsync();
        var userPersons = all.Where(c => c.UserId == request.UserId).ToList();

        await _redisService.SetAsync(cacheKey, userPersons, TimeSpan.FromHours(1));

        return userPersons;
    }
}
