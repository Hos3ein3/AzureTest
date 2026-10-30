using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexMarketOverview;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexMarketOverview
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] ForexMarketFilter filter,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetMarketOverview.HandleAsync(new GetForexMarketOverviewQuery(filter), cancellationToken);
        return result.ToEndpointResult();
    }
}
