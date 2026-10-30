using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.CancelTradingOrder;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class CancelTradingOrder
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CancelTradingOrderRequestDto request,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var command = new CancelTradingOrderCommand(request.OrderId, currentUser.Id, currentUser.TenantId);
        var result = await commands.CancelOrderAsync(command, ct);
        return result.ToEndpointResult();
    }
}
