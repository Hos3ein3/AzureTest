using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.TriggerKillSwitch;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class ActivateCryptoKillSwitch
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CryptoKillSwitchRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        [FromServices] IValidationService validator,
        CancellationToken cancellationToken)
    {
        var command = new TriggerKillSwitchCommand(request.AccountId, request.Reason);
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.TriggerKillSwitch.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
