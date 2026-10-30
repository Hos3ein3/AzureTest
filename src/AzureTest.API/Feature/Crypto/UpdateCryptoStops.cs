using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.UpdateCryptoStops;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class UpdateCryptoStops
{
    public static async Task<IResult> HandleAsync(
        [FromBody] UpdateCryptoStopsRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCryptoStopsCommand(request.PositionId, request.StopLoss, request.TakeProfit, request.TrailingStopDistance);
        var result = await commands.UpdateStops.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
