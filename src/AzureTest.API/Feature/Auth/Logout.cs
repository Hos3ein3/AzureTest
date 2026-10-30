using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.Logout;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Auth;

public static class Logout
{
    public static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        [FromServices] IAuthCommandUnit commands,
        CancellationToken cancellationToken)
    {
        var userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        _ = Guid.TryParse(userIdStr, out var userId);
        var result = await commands.Logout.HandleAsync(new LogoutCommand(userId), cancellationToken);
        return result.ToEndpointResult();
    }
}
