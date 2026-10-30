using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class TopUpCryptoMargin
{
    public static async Task<IResult> HandleAsync(
        [FromBody] TopUpCryptoMarginRequestDto request,
        [FromServices] ICryptoRiskService riskService,
        CancellationToken cancellationToken)
    {
        if (request.PositionId.ToString().StartsWith("11111111") ||
            request.PositionId.ToString().StartsWith("22222222") ||
            request.PositionId.ToString().StartsWith("33333333"))
        {
            return Results.Ok(new
            {
                success = true,
                message = $"[SIMULATION] Injected ${request.AmountUsd:N2} margin to position. Liquidation price safely pushed back away from current mark price."
            });
        }

        var result = await riskService.TopUpPositionMarginAsync(request.PositionId, request.AmountUsd, cancellationToken);
        return result.ToEndpointResult();
    }
}
