using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexMarketOverview;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexSessions
{
    public static async Task<IResult> HandleAsync(
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetMarketOverview.HandleAsync(
            new GetForexMarketOverviewQuery(new ForexMarketFilter()), cancellationToken);

        return result.ToEndpointResult();
    }
}
