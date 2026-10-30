using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class UpdateCryptoRiskLimits
{
    public static async Task<IResult> HandleAsync(
        [FromBody] UpdateCryptoRiskLimitsRequestDto request,
        [FromServices] ICryptoRiskService riskService,
        CancellationToken cancellationToken)
    {
        var result = await riskService.UpdateRiskLimitsAsync(request.AccountId, request.RiskPercentagePerTrade, request.DailyLossLimitPercent, request.MaxOpenPositions, cancellationToken);
        return result.ToEndpointResult();
    }
}
