using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto;
using AzureTest.Application.Crypto.Commands.PlaceCryptoOrder;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Crypto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Crypto;

public static class PlaceCryptoOrder
{
    public static async Task<IResult> HandleAsync(
        [FromBody] PlaceCryptoOrderRequestDto request,
        [FromServices] ICryptoCommandUnit commands,
        [FromServices] IValidationService validator,
        [FromServices] ICurrentUser currentUser,
        CancellationToken cancellationToken)
    {
        var idempotencyKey = request.IdempotencyKey ?? Guid.NewGuid().ToString("N");

        var command = new PlaceCryptoOrderCommand(
            request.AccountId,
            currentUser.Id,
            currentUser.TenantId,
            request.Symbol,
            request.Side,
            request.OrderType,
            request.Quantity,
            request.RequestedPrice,
            request.StopPrice,
            request.Leverage,
            request.StopLoss,
            request.TakeProfit,
            request.TrailingStopDistance,
            idempotencyKey,
            request.TradingMode);

        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.PlaceOrder.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
