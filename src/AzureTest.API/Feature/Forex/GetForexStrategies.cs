using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Queries.GetForexStrategies;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class GetForexStrategies
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] ForexStrategyFilter filter,
        [FromServices] IForexQueryUnit queries,
        CancellationToken cancellationToken)
    {
        var result = await queries.GetStrategies.HandleAsync(new GetForexStrategiesQuery(filter), cancellationToken);
        return result.ToEndpointResult();
    }
}
