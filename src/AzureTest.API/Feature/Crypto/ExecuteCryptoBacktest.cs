using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.RunCryptoBacktest;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class ExecuteCryptoBacktest
{
    public static async Task<IResult> HandleAsync(
        [FromBody] BacktestRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        [FromServices] IValidationService validator,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var command = new RunCryptoBacktestCommand(currentUser.Id, currentUser.TenantId, request);
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.RunBacktest.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
