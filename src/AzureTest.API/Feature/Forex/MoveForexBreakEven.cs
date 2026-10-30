using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.MoveToBreakEven;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public sealed class MoveForexBreakEvenRequest
{
    public Guid TradeId { get; set; }
}

public static class MoveForexBreakEven
{
    public static async Task<IResult> HandleAsync(
        [FromBody] MoveForexBreakEvenRequest request,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.MoveToBreakEven.HandleAsync(new MoveToBreakEvenCommand(request.TradeId), cancellationToken);
        return result.ToEndpointResult();
    }
}
