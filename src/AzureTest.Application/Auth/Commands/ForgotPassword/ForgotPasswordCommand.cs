using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result<ForgotPasswordResult>>;

public sealed record ForgotPasswordResult(
    bool Success,
    string Message,
    string? ResetToken = null
);

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result<ForgotPasswordResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ForgotPasswordResult>> HandleAsync(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Do not reveal that the user does not exist
            return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(
                true,
                "If your email address is registered, instructions to reset your password have been generated."));
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        _logger.LogInformation("Password reset token generated for user {UserId} ({Email}). Token: {Token}",
            user.Id, user.Email, resetToken);

        return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(
            true,
            "Instructions to reset your password have been generated.",
            resetToken));
    }
}
