using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.ActivateKillSwitch;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class ActivateForexKillSwitch
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ForexKillSwitchRequestDto? request,
        [FromServices] IForexCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var reason = request?.Reason ?? "Emergency Manual Kill Switch Triggered";
        var result = await commands.ActivateKillSwitch.HandleAsync(new ActivateKillSwitchCommand(reason), cancellationToken);
        return result.ToEndpointResult();
    }
}
