using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.CreateBacktest;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class ExecuteForexBacktest
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CreateBacktestCommand command,
        [FromServices] IForexCommandUnit commands,
        [FromServices] IValidationService validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.CreateBacktest.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
