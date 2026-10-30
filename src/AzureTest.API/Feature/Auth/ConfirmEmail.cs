using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.ConfirmEmail;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class ConfirmEmail
{
    public static async Task<IResult> HandleAsync(
        [FromQuery] string? userId,
        [FromQuery] string? code,
        [FromServices] IAuthCommandUnit commands,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "Missing userId or confirmation code."
            });
        }

        if (!Guid.TryParse(userId, out _))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "Invalid userId format."
            });
        }

        var result = await commands.ConfirmEmail.HandleAsync(new ConfirmEmailCommand(userId, code), cancellationToken);
        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        if (!result.Value.Succeeded)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Confirmation Failed",
                Detail = result.Value.Message
            });
        }

        return Result<string>.Success(result.Value.Message).ToEndpointResult();
    }
}
