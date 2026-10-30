using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoMarketOverview
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] CryptoMarketFilter filter,
        [FromServices] ICryptoQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetMarketOverview.HandleAsync(new GetCryptoMarketOverviewQuery(filter), cancellationToken);
        return result.ToEndpointResult();
    }
}
