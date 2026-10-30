using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.CloseTradingPosition;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class CloseTradingPosition
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CloseTradingPositionRequestDto request,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var command = new CloseTradingPositionCommand(request.PositionId, currentUser.Id, currentUser.TenantId, request.ClosePrice);
        var result = await commands.ClosePositionAsync(command, ct);
        return result.ToEndpointResult();
    }
}
