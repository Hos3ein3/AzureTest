using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.ForgotPassword;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class ForgotPassword
{
    public static async Task<IResult> HandleAsync(
        [FromBody] ForgotPasswordRequestDto request,
        [FromServices] IAuthCommandUnit commands,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request?.Email))
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "Email address is required."
            });
        }

        var resetLinkTemplate = "/api/v1/auth/reset-password?userId={userId}&code={code}";
        var result = await commands.ForgotPassword.HandleAsync(
            new ForgotPasswordCommand(request.Email, resetLinkTemplate),
            cancellationToken);

        return result.ToEndpointResult();
    }
}
