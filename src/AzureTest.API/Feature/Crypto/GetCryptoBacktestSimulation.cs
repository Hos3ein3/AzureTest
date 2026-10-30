using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class GetCryptoBacktestSimulation
{
    public static IResult Handle(
        [FromQuery] string? symbol,
        [FromQuery] CryptoStrategyType? strategy,
        [FromQuery] CryptoTimeframe? timeframe)
    {
        var model = new BacktestRequestDto
        {
            Symbol = string.IsNullOrWhiteSpace(symbol) ? "BTCUSDT" : symbol,
            StrategyType = strategy ?? CryptoStrategyType.TrendFollowing,
            Timeframe = timeframe ?? CryptoTimeframe.H1,
            DaysBack = 90,
            InitialCapital = 10000m,
            CommissionPercent = 0.075m,
            SlippagePercent = 0.05m,
            FastPeriod = 9,
            SlowPeriod = 21,
            RsiPeriod = 14,
            BbPeriod = 20,
            BbStdDev = 2.0m
        };
        return Results.Ok(model);
    }
}
