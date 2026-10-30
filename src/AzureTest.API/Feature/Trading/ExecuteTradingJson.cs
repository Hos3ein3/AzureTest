using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.ExecuteTradingJsonBatch;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class ExecuteTradingJson
{
    public static async Task<IResult> HandleAsync(
        [FromBody] TradingJsonActionRequestDto request,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Json))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "JSON payload cannot be empty."
            });
        }

        var command = new ExecuteTradingJsonBatchCommand(request.Json, currentUser.Id, currentUser.TenantId);
        var result = await commands.ExecuteJsonBatchAsync(command, ct);
        return result.ToEndpointResult();
    }
}
