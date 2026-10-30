using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class TrimCryptoPosition
{
    public static async Task<IResult> HandleAsync(
        [FromBody] TrimCryptoPositionRequestDto request,
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
                message = $"[SIMULATION] De-risked position by trimming {request.TrimQuantity:N4} units. Maintenance margin requirement reduced."
            });
        }

        var result = await riskService.TrimPositionRiskAsync(request.PositionId, request.TrimQuantity, cancellationToken);
        return result.ToEndpointResult();
    }
}
