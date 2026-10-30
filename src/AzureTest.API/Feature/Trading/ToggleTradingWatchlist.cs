using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.ToggleTradingWatchlist;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class ToggleTradingWatchlist
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ToggleTradingWatchlistRequestDto request,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var command = new ToggleTradingWatchlistCommand(currentUser.Id, currentUser.TenantId, request.Symbol, request.AssetName, request.AssetClass);
        var result = await commands.ToggleWatchlistAsync(command, ct);
        return result.ToEndpointResult();
    }
}
