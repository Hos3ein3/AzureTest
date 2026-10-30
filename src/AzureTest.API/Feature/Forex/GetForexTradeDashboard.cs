using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexTradeDashboard;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexTradeDashboard
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] ForexTradeFilter filter,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetTradeDashboard.HandleAsync(new GetForexTradeDashboardQuery(filter), cancellationToken);
        return result.ToEndpointResult();
    }
}
