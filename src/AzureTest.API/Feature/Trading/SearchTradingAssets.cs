using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class SearchTradingAssets
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] string query,
        [FromServices] ITradingMarketDataService marketData,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result<IReadOnlyList<TradingQuoteDto>>.Success(Array.Empty<TradingQuoteDto>()).ToEndpointResult();
        }

        var quotes = await marketData.SearchAssetsAsync(query.Trim(), ct: ct);
        return Result<IReadOnlyList<TradingQuoteDto>>.Success(quotes).ToEndpointResult();
    }
}
