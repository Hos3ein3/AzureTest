using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoSentiment
{
    public static async Task<IResult> HandleAsync(
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetMarketOverview.HandleAsync(new GetCryptoMarketOverviewQuery(new CryptoMarketFilter()), cancellationToken);
        return result.ToEndpointResult();
    }
}
