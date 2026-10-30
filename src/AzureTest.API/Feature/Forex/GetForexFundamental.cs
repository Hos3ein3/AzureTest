using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexAnalysis;
using AzureTest.Domain.Entities.Forex.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexFundamental
{
    public static async Task<IResult> HandleAsync(
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetAnalysis.HandleAsync(
            new GetForexAnalysisQuery("EUR/USD", ForexTimeframe.H1), cancellationToken);

        return result.ToEndpointResult();
    }
}
