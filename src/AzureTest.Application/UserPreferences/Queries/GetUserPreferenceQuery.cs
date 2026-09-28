using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;

namespace AzureTest.Application.UserPreferences.Queries;

public sealed record GetUserPreferenceQuery(Guid UserId) : IQuery<UserPreference>;

public sealed class GetUserPreferenceQueryHandler : IQueryHandler<GetUserPreferenceQuery, UserPreference>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IRedisService _redisService;

    public GetUserPreferenceQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork, IRedisService redisService)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _redisService = redisService;
    }

    public async Task<UserPreference> HandleAsync(GetUserPreferenceQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"UserPref_{request.UserId}";
        var cached = await _redisService.GetAsync<UserPreference>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var pref = await _noSqlUnitOfWork.UserPreferences.GetByUserIdAsync(request.UserId, cancellationToken);
        if (pref == null)
        {
            pref = new UserPreference { UserId = request.UserId };
        }
        else
        {
            await _redisService.SetAsync(cacheKey, pref, TimeSpan.FromHours(1));
        }

        return pref;
    }
}
