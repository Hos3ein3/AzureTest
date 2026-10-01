

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Companies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

public class CompanyCacheService : ICompanyCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CompanyCacheService> _logger;
    private const string CacheKey = "CompaniesList";

    public CompanyCacheService(IDistributedCache cache, ILogger<CompanyCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<CompanyDto>?> GetCompaniesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(CacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogInformation("Retrieved companies from Redis cache.");
                return JsonSerializer.Deserialize<IEnumerable<CompanyDto>>(cachedData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve companies from Redis cache.");
        }

        return null;
    }

    public async Task SetCompaniesAsync(IEnumerable<CompanyDto> companies, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };

            var serializedData = JsonSerializer.Serialize(companies);
            await _cache.SetStringAsync(CacheKey, serializedData, options, cancellationToken);
            _logger.LogInformation("Successfully cached companies in Redis.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set companies in Redis cache.");
        }
    }

    public async Task InvalidateCompaniesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(CacheKey, cancellationToken);
            _logger.LogInformation("Invalidated companies in Redis cache.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate companies in Redis cache.");
        }
    }
}
