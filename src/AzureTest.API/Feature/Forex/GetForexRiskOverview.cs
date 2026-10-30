using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexRiskAssessment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexRiskOverview
{
    public static async Task<IResult> HandleAsync(
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetRiskAssessment.HandleAsync(new GetForexRiskAssessmentQuery(), cancellationToken);
        return result.ToEndpointResult();
    }
}
