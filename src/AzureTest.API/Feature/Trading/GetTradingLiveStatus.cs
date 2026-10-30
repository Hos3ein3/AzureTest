using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.SyncTradingPrices;
using AzureTest.Application.Trading.Queries.GetTradingDashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class GetTradingLiveStatus
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] string? symbol,
        ClaimsPrincipal user,
        [FromServices] ITradingQueryUnit queries,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (user.Identity?.IsAuthenticated == true)
        {
            try
            {
                await commands.SyncPricesAsync(new SyncTradingPricesCommand(), ct);
            }
            catch
            {
                // Background price sync best effort
            }
        }

        var result = await queries.GetDashboardAsync(
            new GetTradingDashboardQuery(currentUser.Id, currentUser.TenantId, symbol ?? "BTCUSDT"), ct);

        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        var dto = result.Value;
        var response = new
        {
            success = true,
            account = dto.Account,
            openPositions = dto.OpenPositions,
            closedPositions = dto.ClosedPositions,
            pendingOrders = dto.PendingOrders,
            selectedQuote = dto.SelectedQuote
        };

        return Results.Ok(response);
    }
}
