using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.PlaceCryptoSmartOrder;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class PlaceCryptoSmartOrder
{
    public static async Task<IResult> HandleAsync(
        [FromBody] PlaceCryptoSmartOrderRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var command = new PlaceCryptoSmartOrderCommand(
            request.AccountId,
            currentUser.Id,
            currentUser.TenantId,
            request.Symbol,
            request.Side,
            request.SmartOrderType,
            request.TotalQuantity,
            request.RequestedPrice,
            request.DurationMinutes,
            request.IntervalMinutes,
            request.VisibleQuantity,
            request.VariancePercent,
            request.TrailingDistanceUsd,
            request.PositionId);

        var result = await commands.PlaceSmartOrder.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
