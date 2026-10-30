using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Queries.GetTradingQuotes;
using AzureTest.Domain.Entities.Trading.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class GetTradingQuotes
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] AssetClass? assetClass,
        [FromQuery] string? search,
        [FromServices] ITradingQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var result = await queries.GetQuotesAsync(
            new GetTradingQuotesQuery(currentUser.Id, currentUser.TenantId, assetClass, search), ct);

        return result.ToEndpointResult();
    }
}
