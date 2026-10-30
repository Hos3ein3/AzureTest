using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoScreener;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoScreener
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoScreenerFilterDto filter,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetScreener.HandleAsync(new GetCryptoScreenerQuery(), cancellationToken);
        if (result.IsSuccess)
        {
            var list = result.Value.AsEnumerable();
            if (filter.MinVolume.HasValue == true) list = list.Where(x => x.Volume24h >= filter.MinVolume.Value);
            if (filter.MinRsi.HasValue == true) list = list.Where(x => x.Rsi >= filter.MinRsi.Value);
            if (filter.MaxRsi.HasValue == true) list = list.Where(x => x.Rsi <= filter.MaxRsi.Value);
            if (filter.MinChange.HasValue == true) list = list.Where(x => x.Change24h >= filter.MinChange.Value);
            if (filter.MaxChange.HasValue == true) list = list.Where(x => x.Change24h <= filter.MaxChange.Value);
            if (!string.IsNullOrWhiteSpace(filter.Trend)) list = list.Where(x => x.Recommendation.Contains(filter.Trend, System.StringComparison.OrdinalIgnoreCase));
            result = Result<IReadOnlyList<CryptoScreenerItemDto>>.Success(list.ToList());
        }

        return result.ToEndpointResult();
    }
}
