using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Queries.GetTradingHistory;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class GetTradingHistory
{
    public static async Task<IResult> HandleAsync(
        [AsParameters] TradingHistoryFilterDto filter,
        [FromServices] ITradingQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var result = await queries.GetHistoryAsync(
            new GetTradingHistoryQuery(
                currentUser.Id,
                currentUser.TenantId,
                filter.Search,
                filter.Side,
                filter.AssetClass,
                filter.Outcome), ct);

        return result.ToEndpointResult();
    }
}
