using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using Microsoft.Extensions.Caching.Distributed;

namespace AzureTest.Application.IdentitySettings;

public sealed record GetIdentitySettingQuery() : IQuery<Result<IdentitySetting>>;

public sealed class GetIdentitySettingQueryHandler : IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>>
{
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly IDistributedCache _cache;
    private const string CacheKey = "IdentitySetting_Cache";

    public GetIdentitySettingQueryHandler(INoSqlUnitOfWork noSqlUnitOfWork, IDistributedCache cache)
    {
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _cache = cache;
    }

    public async Task<Result<IdentitySetting>> HandleAsync(GetIdentitySettingQuery request, CancellationToken cancellationToken = default)
    {
        // 1. Try Redis
        var cached = await _cache.GetStringAsync(CacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            var settingFromCache = JsonSerializer.Deserialize<IdentitySetting>(cached);
            if (settingFromCache != null)
                return Result<IdentitySetting>.Success(settingFromCache);
        }

        // 2. Fetch from Mongo
        var list = await _noSqlUnitOfWork.IdentitySettings.GetAllAsync();
        var setting = list.Count > 0 ? list[0] : null;

        if (setting == null)
            return Result<IdentitySetting>.Failure("Identity setting not found.");

        // 3. Save to Redis
        await _cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(setting), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) }, cancellationToken);

        return Result<IdentitySetting>.Success(setting);
    }
}