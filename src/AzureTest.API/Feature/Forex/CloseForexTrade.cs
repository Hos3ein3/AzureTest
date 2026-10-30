using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.CloseTrade;
using AzureTest.Domain.Entities.Forex.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public sealed class CloseForexTradeRequest
{
    public Guid TradeId { get; set; }
}

public static class CloseForexTrade
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CloseForexTradeRequest request,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.CloseTrade.HandleAsync(
            new CloseTradeCommand(request.TradeId, ForexExitReason.Manual), cancellationToken);

        return result.ToEndpointResult();
    }
}
