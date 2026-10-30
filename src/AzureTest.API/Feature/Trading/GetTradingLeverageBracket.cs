using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class GetTradingLeverageBracket
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] string? symbol,
        [FromServices] ITradingMarketDataService marketData,
        CancellationToken ct)
    {
        var targetSymbol = string.IsNullOrWhiteSpace(symbol) ? "BTCUSDT" : symbol.Trim();
        var bracket = await marketData.GetLeverageBracketAsync(targetSymbol, ct);
        return Result<TradingLeverageBracketDto>.Success(bracket).ToEndpointResult();
    }
}
