using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid Id) : ICommand<(bool Success, string[] Errors)>;

internal sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand, (bool Success, string[] Errors)>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(UserManager<ApplicationUser> userManager, ILogger<DeleteUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<(bool Success, string[] Errors)> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(command.Id.ToString());
        if (user == null)
            return (false, new[] { "User not found." });

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(result.Errors, e => e.Description));
            return (false, errors);
        }

        _logger.LogInformation("Admin deleted user {UserId}", command.Id);
        return (true, Array.Empty<string>());
    }
}
