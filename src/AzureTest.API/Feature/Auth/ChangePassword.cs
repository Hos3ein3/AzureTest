using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.ChangePassword;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class ChangePassword
{
    public static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        [FromBody] ChangePasswordRequestDto request,
        [FromServices] IAuthCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdStr, out var userId) || userId == Guid.Empty)
        {
            return Results.Unauthorized();
        }

        var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword, request.ConfirmPassword);
        var result = await commands.ChangePassword.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        if (!result.Value.Success)
        {
            var msg = result.Value.Errors != null && result.Value.Errors.Count > 0
                ? string.Join("; ", result.Value.Errors)
                : "Failed to change password.";

            return Results.BadRequest(new ProblemDetails
            {
                Title = "Change Password Failed",
                Detail = msg
            });
        }

        return Result<string>.Success("Password changed successfully.").ToEndpointResult();
    }
}
