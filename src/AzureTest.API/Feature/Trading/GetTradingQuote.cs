using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.SyncTradingPrices;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class GetTradingQuote
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] string symbol,
        ClaimsPrincipal user,
        [FromServices] ITradingMarketDataService marketData,
        [FromServices] ITradingCommandUnit commands,
        CancellationToken ct)
    {
        var quote = await marketData.GetQuoteAsync(symbol, ct: ct);
        if (quote == null)
        {
            return Results.NotFound(new ProblemDetails
            {
                Title = "Not Found",
                Detail = "Asset quote not found."
            });
        }

        if (user.Identity?.IsAuthenticated == true)
        {
            try
            {
                await commands.SyncPricesAsync(new SyncTradingPricesCommand(), ct);
            }
            catch
            {
                // Background price sync best effort
            }
        }

        return Result<TradingQuoteDto>.Success(quote).ToEndpointResult();
    }
}
