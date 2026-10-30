using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.ResetDemoAccount;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class ResetForexAccount
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ResetDemoAccountCommand command,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var result = await commands.ResetDemoAccount.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
