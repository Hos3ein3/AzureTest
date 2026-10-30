using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetBacktestDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexBacktestResults
{
    public static async Task<IResult> HandleAsync(
        [FromRoute] Guid id,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetBacktestDetails.HandleAsync(new GetBacktestDetailsQuery(id), cancellationToken);
        return result.ToEndpointResult();
    }
}
