using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.UpdateTradeStops;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class UpdateForexStops
{
    public static async Task<IResult> HandleAsync(
        [FromBody] UpdateTradeStopsCommand command,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.UpdateTradeStops.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
