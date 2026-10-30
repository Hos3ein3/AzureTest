using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoCandles;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoChart
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoChartRequestDto request,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetCandles.HandleAsync(
            new GetCryptoCandlesQuery(request.Symbol, request.Timeframe, 120), cancellationToken);

        return result.ToEndpointResult();
    }
}
