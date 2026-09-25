using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Users.Commands.AssignRolesToUser;

public sealed record AssignRolesToUserCommand(Guid UserId, string[] Roles) : ICommand<(bool Success, string[] Errors)>;

internal sealed class AssignRolesToUserCommandHandler : ICommandHandler<AssignRolesToUserCommand, (bool Success, string[] Errors)>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AssignRolesToUserCommandHandler> _logger;

    public AssignRolesToUserCommandHandler(UserManager<ApplicationUser> userManager, ILogger<AssignRolesToUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<(bool Success, string[] Errors)> HandleAsync(AssignRolesToUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
            return (false, new[] { "User not found." });

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Remove old roles
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(removeResult.Errors, e => e.Description));
            return (false, errors);
        }

        // Add new roles
        if (command.Roles != null && command.Roles.Length > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, command.Roles);
            if (!addResult.Succeeded)
            {
                var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(addResult.Errors, e => e.Description));
                return (false, errors);
            }
        }

        _logger.LogInformation("Admin updated roles for user {UserId}", command.UserId);
        return (true, Array.Empty<string>());
    }
}
