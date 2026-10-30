using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.ModifyTradingPosition;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class ModifyTradingPosition
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ModifyTradingPositionRequestDto request,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var command = new ModifyTradingPositionCommand(
            request.PositionId,
            currentUser.Id,
            currentUser.TenantId,
            request.StopLoss,
            request.TakeProfit,
            request.TrailingStopDistance,
            request.MultipleTakeProfitsJson);

        var result = await commands.ModifyPositionAsync(command, ct);
        return result.ToEndpointResult();
    }
}
