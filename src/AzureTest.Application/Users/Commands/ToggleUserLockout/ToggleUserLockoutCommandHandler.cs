using AzureTest.Application.Abstractions;


using FluentValidation;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AzureTest.Application.Users.Commands.ToggleUserLockout;

public sealed record ToggleUserLockoutCommand(
    Guid Id,
    DateTimeOffset? LockoutEnd = null,
    bool IsPermanent = false,
    bool Unlock = false
) : ICommand<(bool Success, string[] Errors)>;

internal sealed class ToggleUserLockoutCommandHandler : ICommandHandler<ToggleUserLockoutCommand, (bool Success, string[] Errors)>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ToggleUserLockoutCommandHandler> _logger;

    public ToggleUserLockoutCommandHandler(UserManager<ApplicationUser> userManager, ILogger<ToggleUserLockoutCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<(bool Success, string[] Errors)> HandleAsync(ToggleUserLockoutCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(command.Id.ToString());
        if (user == null)
            return (false, new[] { "User not found." });

        IdentityResult result;
        if (command.Unlock)
        {
            // Explicit unlock
            result = await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
        }
        else if (command.IsPermanent)
        {
            // Permanent lockout in UTC for PostgreSQL
            var permanentDate = DateTimeOffset.UtcNow.AddYears(100);
            result = await _userManager.SetLockoutEndDateAsync(user, permanentDate);
        }
        else if (command.LockoutEnd.HasValue)
        {
            // Convert strictly to UTC for PostgreSQL
            var utcLockout = command.LockoutEnd.Value.ToUniversalTime();
            result = await _userManager.SetLockoutEndDateAsync(user, utcLockout);
        }
        else
        {
            // Fallback toggle
            var isCurrentlyLockedOut = await _userManager.IsLockedOutAsync(user);
            if (isCurrentlyLockedOut)
            {
                result = await _userManager.SetLockoutEndDateAsync(user, null);
                await _userManager.ResetAccessFailedCountAsync(user);
            }
            else
            {
                result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }
        }

        if (!result.Succeeded)
        {
            var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(result.Errors, e => e.Description));
            return (false, errors);
        }

        _logger.LogInformation("Admin modified lockout for user {UserId}. LockoutEnd: {LockoutEnd}", command.Id, user.LockoutEnd);
        return (true, Array.Empty<string>());
    }
}

public sealed class ToggleUserLockoutCommandValidator : AbstractValidator<ToggleUserLockoutCommand>
{
    public ToggleUserLockoutCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
    }
}
