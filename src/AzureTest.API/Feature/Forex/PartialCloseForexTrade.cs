using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.PartialCloseTrade;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class PartialCloseForexTrade
{
    public static async Task<IResult> HandleAsync(
        [FromBody] PartialCloseForexTradeRequestDto request,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.PartialCloseTrade.HandleAsync(
            new PartialCloseTradeCommand(request.TradeId, request.LotsToClose), cancellationToken);

        return result.ToEndpointResult();
    }
}
