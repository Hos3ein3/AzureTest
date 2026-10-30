using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.DeleteCryptoAlert;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class DeleteCryptoAlert
{
    public static async Task<IResult> HandleAsync(
        [FromRoute] Guid alertId,
        [FromServices] ICryptoCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCryptoAlertCommand(alertId);
        var result = await commands.DeleteAlert.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
