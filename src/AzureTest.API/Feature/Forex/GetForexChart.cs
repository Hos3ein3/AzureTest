using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexCandles;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexChart
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] ForexChartRequestDto request,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetCandles.HandleAsync(
            new GetForexCandlesQuery(request.Symbol, request.Timeframe, 120), cancellationToken);

        return result.ToEndpointResult();
    }
}
