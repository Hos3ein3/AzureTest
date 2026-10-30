using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.CancelOrder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public sealed class CancelForexOrderRequest
{
    public Guid OrderId { get; set; }
}

public static class CancelForexOrder
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CancelForexOrderRequest request,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CancelOrder.HandleAsync(new CancelOrderCommand(request.OrderId), cancellationToken);
        return result.ToEndpointResult();
    }
}
