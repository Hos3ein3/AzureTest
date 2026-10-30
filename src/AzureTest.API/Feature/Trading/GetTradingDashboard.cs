using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Queries.GetTradingDashboard;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class GetTradingDashboard
{
    public static async Task<IResult> HandleAsync(
        [FromServices] ITradingQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var result = await queries.GetDashboardAsync(
            new GetTradingDashboardQuery(currentUser.Id, currentUser.TenantId, "BTCUSDT"), ct);

        return result.ToEndpointResult();
    }
}
