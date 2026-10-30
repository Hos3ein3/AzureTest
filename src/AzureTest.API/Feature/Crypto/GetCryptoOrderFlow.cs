using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketDataHub;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoOrderFlow
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoChartRequestDto request,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetMarketDataHub.HandleAsync(
            new GetCryptoMarketDataHubQuery(request.Symbol, request.Timeframe), cancellationToken);

        return result.ToEndpointResult();
    }
}
