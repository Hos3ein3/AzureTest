using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class ExecuteCryptoArbitrage
{
    public static IResult Handle(
        [FromBody] ExecuteArbitrageRequestDto request)
    {
        return Results.Ok(new
        {
            success = true,
            pairOrRoute = request.PairOrRoute,
            type = request.Type,
            expectedProfit = request.ExpectedProfit,
            message = $"Arbitrage loop for {request.PairOrRoute} ({request.Type}) triggered successfully! Estimated net yield: +${request.ExpectedProfit:N2}."
        });
    }
}
