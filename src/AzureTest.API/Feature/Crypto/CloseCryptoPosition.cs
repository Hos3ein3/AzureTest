using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.CloseCryptoPosition;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class CloseCryptoPosition
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CloseCryptoPositionRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var command = new CloseCryptoPositionCommand(request.PositionId, request.ClosePrice);
        var result = await commands.ClosePosition.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
