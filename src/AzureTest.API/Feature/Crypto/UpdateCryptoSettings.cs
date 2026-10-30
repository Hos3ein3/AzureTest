using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.UpdateCryptoSettings;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class UpdateCryptoSettings
{
    public static async Task<IResult> HandleAsync(
        [FromBody] UpdateCryptoSettingsRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        [FromServices] IValidationService validator,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCryptoSettingsCommand(
            currentUser.Id,
            currentUser.TenantId,
            request.Exchange,
            request.ApiKey,
            request.ApiSecret,
            request.Passphrase,
            request.IsTestnet);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.UpdateSettings.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
