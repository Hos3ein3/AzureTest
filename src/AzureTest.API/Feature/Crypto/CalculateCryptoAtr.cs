using AzureTest.Application.Crypto.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class CalculateCryptoAtr
{
    public static IResult Handle(
        [FromBody] CalculateCryptoAtrRequestDto request,
        [FromServices] ICryptoRiskService riskService)
    {
        var dto = riskService.CalculateAtrPositionSize(request.AccountEquity, request.RiskPercent, request.Symbol, request.EntryPrice, request.AtrMultiplier, request.CustomAtr);
        return Results.Ok(dto);
    }
}
