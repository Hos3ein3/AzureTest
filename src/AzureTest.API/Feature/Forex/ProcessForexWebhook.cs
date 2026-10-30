using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.ProcessTradingViewWebhook;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class ProcessForexWebhook
{
    public static async Task<IResult> HandleAsync(
        [FromBody] TradingViewWebhookPayloadDto payload,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        if (payload == null)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "Empty webhook payload."
            });
        }

        var result = await commands.ProcessTradingViewWebhook.HandleAsync(
            new ProcessTradingViewWebhookCommand(payload), cancellationToken);

        return result.ToEndpointResult();
    }
}
