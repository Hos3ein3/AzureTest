using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(
    string UserId,
    string Code,
    string Password,
    string ConfirmPassword
) : ICommand<Result<ResetPasswordResult>>;

public sealed record ResetPasswordResult(
    bool Succeeded,
    string Message,
    IReadOnlyList<string>? Errors = null
);

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

public sealed class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, Result<ResetPasswordResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ResetPasswordResult>> HandleAsync(
        ResetPasswordCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<ResetPasswordResult>.Success(new ResetPasswordResult(
                false,
                "User not found or invalid reset request.",
                new[] { "Invalid user." }));
        }

        string decodedCode;
        try
        {
            var decodedBytes = WebEncoders.Base64UrlDecode(request.Code);
            decodedCode = Encoding.UTF8.GetString(decodedBytes);
        }
        catch
        {
            decodedCode = request.Code;
        }

        var identityResult = await _userManager.ResetPasswordAsync(user, decodedCode, request.Password);

        // If base64-decoded token failed, try raw code fallback
        if (!identityResult.Succeeded && decodedCode != request.Code)
        {
            identityResult = await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
        }

        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Password reset failed for user {UserId}: {Errors}",
                user.Id, string.Join("; ", errors));

            return Result<ResetPasswordResult>.Success(new ResetPasswordResult(
                false,
                "Password reset failed. The link may have expired or is invalid.",
                errors));
        }

        _logger.LogInformation("Password reset successfully for user {UserId} ({Email})", user.Id, user.Email);

        return Result<ResetPasswordResult>.Success(new ResetPasswordResult(
            true,
            "Your password has been reset successfully. You can now sign in with your new password."));
    }
}
