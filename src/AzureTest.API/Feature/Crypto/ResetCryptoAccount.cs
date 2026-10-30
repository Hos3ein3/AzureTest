using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.ResetCryptoAccount;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class ResetCryptoAccount
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ResetCryptoAccountRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var command = new ResetCryptoAccountCommand(request.AccountId, request.InitialBalance);
        var result = await commands.ResetAccount.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
