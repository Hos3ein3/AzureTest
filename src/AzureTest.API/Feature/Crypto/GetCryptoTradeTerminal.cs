using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Queries.GetCryptoTradeTerminal;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoTradeTerminal
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] string? symbol,
        [FromServices] ICryptoQueryUnit queries,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var sym = string.IsNullOrWhiteSpace(symbol) ? "BTCUSDT" : symbol;
        var result = await queries.GetTradeTerminal.HandleAsync(new GetCryptoTradeTerminalQuery(currentUser.Id, sym), cancellationToken);
        return result.ToEndpointResult();
    }
}
