using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.ResetTradingAccountBalance;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class ResetTradingBalance
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ResetTradingBalanceRequestDto request,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var command = new ResetTradingAccountBalanceCommand(currentUser.Id, currentUser.TenantId, request.NewBalance);
        var result = await commands.ResetAccountBalanceAsync(command, ct);
        return result.ToEndpointResult();
    }
}
