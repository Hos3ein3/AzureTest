using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Users.Commands.AdminResetPassword;

public sealed record AdminResetPasswordCommand(
    Guid UserId,
    string NewPassword,
    string ConfirmPassword
) : ICommand<(bool Success, string[] Errors)>;

public sealed class AdminResetPasswordCommandHandler : ICommandHandler<AdminResetPasswordCommand, (bool Success, string[] Errors)>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminResetPasswordCommandHandler> _logger;

    public AdminResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<AdminResetPasswordCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<(bool Success, string[] Errors)> HandleAsync(
        AdminResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return (false, new[] { "User not found." });
        }

        IdentityResult result;
        if (await _userManager.HasPasswordAsync(user))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, resetToken, command.NewPassword);
        }
        else
        {
            // OAuth or external user without existing local password
            result = await _userManager.AddPasswordAsync(user, command.NewPassword);
        }

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            _logger.LogWarning("Admin password reset failed for user {UserId}: {Errors}", user.Id, string.Join("; ", errors));
            return (false, errors);
        }

        // Reset failed attempt count and refresh security stamp
        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        _logger.LogInformation("Admin successfully reset password for user {UserId} ({Email})", user.Id, user.Email);
        return (true, Array.Empty<string>());
    }
}

public sealed class AdminResetPasswordCommandValidator : AbstractValidator<AdminResetPasswordCommand>
{
    public AdminResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmation password is required.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
