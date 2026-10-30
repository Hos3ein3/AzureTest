using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.ResetPassword;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class ResetPassword
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ResetPasswordRequestDto request,
        [FromServices] IAuthCommandUnit commands,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "Password and confirmation password are required."
            });
        }

        if (request.Password != request.ConfirmPassword)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "Passwords do not match."
            });
        }

        var command = new ResetPasswordCommand(request.UserId, request.Code, request.Password, request.ConfirmPassword);
        var result = await commands.ResetPassword.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        if (!result.Value.Succeeded)
        {
            var msg = result.Value.Errors != null && result.Value.Errors.Count > 0
                ? string.Join("; ", result.Value.Errors)
                : result.Value.Message;

            return Results.BadRequest(new ProblemDetails
            {
                Title = "Password Reset Failed",
                Detail = msg
            });
        }

        return Result<string>.Success("Password has been reset successfully.").ToEndpointResult();
    }
}
