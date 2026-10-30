using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexAnalysis;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexAnalysis
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] ForexAnalysisRequestDto request,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetAnalysis.HandleAsync(
            new GetForexAnalysisQuery(request.Symbol, request.Timeframe), cancellationToken);

        return result.ToEndpointResult();
    }
}
