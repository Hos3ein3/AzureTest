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

public sealed record ToggleUserLockoutCommand(Guid Id) : ICommand<(bool Success, string[] Errors)>;
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
        var isCurrentlyLockedOut = await _userManager.IsLockedOutAsync(user);
        IdentityResult result;
        if (isCurrentlyLockedOut)
        {
            // Unlock
            result = await _userManager.SetLockoutEndDateAsync(user, null);
        }
        else
        {
            // Lock out indefinitely (e.g. 100 years)
            result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        if (!result.Succeeded)
        {
            var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(result.Errors, e => e.Description));
            return (false, errors);
        }
        _logger.LogInformation("Admin toggled lockout for user {UserId}. Now locked: {IsLockedOut}", command.Id, !isCurrentlyLockedOut);
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
