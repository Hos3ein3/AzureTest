using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.GoogleCallback;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public sealed record GoogleAuthRequestDto(
    string? IdToken = null,
    string? Email = null,
    string? Name = null,
    string? ProviderKey = null,
    GoogleAuthMode Mode = GoogleAuthMode.SignInOrRegister
);

public static class GoogleAuth
{
    public static async Task<IResult> HandleAsync(
        [FromBody] GoogleAuthRequestDto request,
        [FromServices] IAuthCommandUnit commands,
        [FromServices] IUserManagerService userManagerService,
        CancellationToken cancellationToken)
    {
        var email = request.Email?.Trim();
        var name = request.Name?.Trim() ?? "Google User";
        var parts = name.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var givenName = parts.Length > 0 ? parts[0] : "Google";
        var surname = parts.Length > 1 ? parts[1] : "User";

        var providerKey = !string.IsNullOrWhiteSpace(request.ProviderKey)
            ? request.ProviderKey
            : (!string.IsNullOrWhiteSpace(request.IdToken) ? request.IdToken.GetHashCode().ToString("x8") : Guid.NewGuid().ToString("N"));

        if (string.IsNullOrWhiteSpace(email))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Google Authentication Failed",
                Detail = "A valid Google email address is required."
            });
        }

        var command = new ProcessGoogleCallbackCommand(
            LoginProvider: "Google",
            ProviderKey: providerKey,
            Email: email,
            GivenName: givenName,
            Surname: surname,
            Mode: request.Mode,
            ReturnUrl: "/trading"
        );

        var result = await commands.GoogleCallback.HandleAsync(command, cancellationToken);
        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        var callbackResult = result.Value;
        if (!callbackResult.Succeeded || callbackResult.User == null)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = callbackResult.IsLockedOut ? "Account Locked" : "Google Authentication Failed",
                Detail = callbackResult.ErrorMessage ?? "Google authentication was unsuccessful."
            });
        }

        var tokensDto = await userManagerService.GenerateJwtTokenAsync(callbackResult.User.Email!);

        var response = new LoginResponseDto(
            Succeeded: true,
            IsLockedOut: false,
            Token: tokensDto.Token,
            RefreshToken: tokensDto.RefreshToken,
            RedirectUrl: "/trading",
            Message: "Google authentication successful."
        );

        return Result<LoginResponseDto>.Success(response).ToEndpointResult();
    }
}
