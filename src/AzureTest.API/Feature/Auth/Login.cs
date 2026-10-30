using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.Login;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class Login
{
    public static async Task<IResult> HandleAsync(
        [FromBody] LoginRequestDto request,
        [FromServices] IAuthCommandUnit commands,
        [FromServices] IUserManagerService userManagerService,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password, request.RememberMe);
        var result = await commands.Login.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        var loginResult = result.Value;
        if (!loginResult.Succeeded)
        {
            var message = loginResult.Errors != null && loginResult.Errors.Count > 0
                ? string.Join("; ", loginResult.Errors)
                : "Invalid email or password.";

            return Results.BadRequest(new ProblemDetails
            {
                Title = loginResult.IsLockedOut ? "Account Locked" : "Login Failed",
                Detail = message
            });
        }

        // Ensure JWT tokens are always populated for API callers
        var token = loginResult.Token;
        var refreshToken = loginResult.RefreshToken;

        if (string.IsNullOrEmpty(token) && loginResult.User != null && !string.IsNullOrEmpty(loginResult.User.Email))
        {
            var tokensDto = await userManagerService.GenerateJwtTokenAsync(loginResult.User.Email);
            token = tokensDto.Token;
            refreshToken = tokensDto.RefreshToken;
        }

        var response = new LoginResponseDto(
            Succeeded: true,
            IsLockedOut: false,
            Token: token,
            RefreshToken: refreshToken,
            RedirectUrl: loginResult.RedirectUrl,
            Message: "Login successful.",
            LockoutEnd: loginResult.LockoutEnd
        );

        return Result<LoginResponseDto>.Success(response).ToEndpointResult();
    }
}
