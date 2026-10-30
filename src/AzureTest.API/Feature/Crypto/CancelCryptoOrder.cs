using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.CancelCryptoOrder;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class CancelCryptoOrder
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CancelCryptoOrderRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var command = new CancelCryptoOrderCommand(request.OrderId);
        var result = await commands.CancelOrder.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
