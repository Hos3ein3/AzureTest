using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.Register;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class Register
{
    public static async Task<IResult> HandleAsync(
        [FromBody] RegisterRequestDto request,
        [FromServices] IAuthCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var confirmationTemplate = "/api/v1/auth/confirm-email?userId={userId}&code={code}";
        var command = new RegisterCommand(request.Name, request.Email, request.Password, request.ConfirmPassword, confirmationTemplate);
        var result = await commands.Register.HandleAsync(command, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToEndpointResult();
        }

        var regResult = result.Value;
        if (!regResult.Succeeded)
        {
            var msg = regResult.Errors != null && regResult.Errors.Count > 0
                ? string.Join("; ", regResult.Errors)
                : "Registration failed.";

            return Results.BadRequest(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = msg
            });
        }

        return Result<string>.Success(regResult.ConfirmationEmailSent
            ? $"Registration successful! A confirmation email has been sent to {request.Email}."
            : "Registration successful.").ToEndpointResult();
    }
}
