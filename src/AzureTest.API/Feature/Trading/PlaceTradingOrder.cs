using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Trading;
using AzureTest.Application.Trading.Commands.PlaceTradingOrder;
using AzureTest.Contracts.DTOs.Trading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Trading;

public static class PlaceTradingOrder
{
    public static async Task<IResult> HandleAsync(
        [FromBody] PlaceTradingOrderRequestDto request,
        [FromServices] ITradingCommandUnit commands,
        [FromServices] IValidationService validator,
        [FromServices] ICurrentUser currentUser,
        CancellationToken ct)
    {
        var command = new PlaceTradingOrderCommand(
            currentUser.Id,
            currentUser.TenantId,
            request.Symbol,
            request.AssetName,
            request.AssetClass,
            request.Side,
            request.OrderType,
            request.Quantity,
            request.Price,
            request.TriggerPrice,
            request.Leverage,
            request.StopLoss,
            request.TakeProfit,
            request.MultipleTakeProfitsJson,
            request.TrailingStopDistance,
            request.TimeInForce,
            request.ExpiryDateUtc);

        var validation = await validator.ValidateAsync(command, ct);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.PlaceOrderAsync(command, ct);
        return result.ToEndpointResult();
    }
}
