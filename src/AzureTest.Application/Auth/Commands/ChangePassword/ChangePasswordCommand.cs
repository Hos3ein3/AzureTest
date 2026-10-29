using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Auth.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
) : ICommand<Result<ChangePasswordResult>>;

public sealed record ChangePasswordResult(
    bool Success,
    IReadOnlyList<string>? Errors = null
);

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("The new password and confirmation password do not match.");
    }
}

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthSessionService _sessionService;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IAuthSessionService sessionService,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ChangePasswordResult>> HandleAsync(
        ChangePasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Result<ChangePasswordResult>.Failure("User account was not found.");
        }

        var identityResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            return Result<ChangePasswordResult>.Success(new ChangePasswordResult(false, errors));
        }

        // Update security stamp and refresh cached session
        await _userManager.UpdateSecurityStampAsync(user);
        await _sessionService.InvalidateUserSessionAsync(user.Id, cancellationToken);
        await _sessionService.CacheUserSessionAsync(user, cancellationToken);

        _logger.LogInformation("Password changed successfully for user {UserId}", user.Id);
        return Result<ChangePasswordResult>.Success(new ChangePasswordResult(true));
    }
}
