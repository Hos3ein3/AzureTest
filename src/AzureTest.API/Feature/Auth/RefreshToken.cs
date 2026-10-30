using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class RefreshToken
{
    public static async Task<IResult> HandleAsync(
        [FromBody] RefreshTokenRequestDto request,
        [FromServices] IAuthCommandUnit commands,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "Both token and refreshToken must be provided."
            });
        }

        var result = await commands.RefreshToken.HandleAsync(
            new RefreshTokenCommand(request.Token, request.RefreshToken),
            cancellationToken);

        if (result.IsFailure || !result.Value.Success)
        {
            return Results.Unauthorized();
        }

        return Result<RefreshTokenResult>.Success(result.Value).ToEndpointResult();
    }
}
