using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex;
using AzureTest.Application.Forex.Commands.PlaceOrder;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Forex;

public static class PlaceForexOrder
{
    public static async Task<IResult> HandleAsync(
        [FromBody] PlaceOrderCommand command,
        [FromServices] IForexCommandUnit commands,
        [FromServices] IValidationService validator,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (validation.IsFailure)
        {
            return validation.ToEndpointResult();
        }

        var result = await commands.PlaceOrder.HandleAsync(command, cancellationToken);
        return result.ToEndpointResult();
    }
}
