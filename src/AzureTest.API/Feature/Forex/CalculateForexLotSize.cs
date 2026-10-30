using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Contracts.DTOs.Forex;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class CalculateForexLotSize
{
    public static IResult Handle(
        [AsParameters] LotSizeCalculationRequestDto request,
        [FromServices] IForexRiskService riskService)
    {
        if (request.AccountBalance <= 0 && string.IsNullOrWhiteSpace(request.Symbol))
        {
            request = new LotSizeCalculationRequestDto(10_000m, 1.0m, 25m, "EUR/USD", 30m);
        }
        var result = riskService.CalculateLotSize(request);
        return Result<LotSizeCalculationResultDto>.Success(result).ToEndpointResult();
    }
}
