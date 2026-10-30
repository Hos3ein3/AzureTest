using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.CreateCryptoAlert;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class CreateCryptoAlert
{
    public static async Task<IResult> HandleAsync(
        [FromBody] CreateCryptoAlertRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        [FromServices] IValidationService validator,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var command = new CreateCryptoAlertCommand(
            currentUser.Id,
            currentUser.TenantId,
            request.Symbol,
            request.AlertType,
            request.TargetValue,
            request.Message,
            request.Channel ?? "InApp");

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.CreateAlert.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
