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

namespace AzureTest.Application.Auth.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    string UserId,
    string Code
) : ICommand<Result<ConfirmEmailResult>>;

public sealed record ConfirmEmailResult(
    bool Succeeded,
    string Message,
    IReadOnlyList<string>? Errors = null
);

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Confirmation token is required.");
    }
}

public sealed class ConfirmEmailCommandHandler : ICommandHandler<ConfirmEmailCommand, Result<ConfirmEmailResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ConfirmEmailResult>> HandleAsync(
        ConfirmEmailCommand request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result<ConfirmEmailResult>.Success(new ConfirmEmailResult(
                false,
                "User not found or invalid confirmation link.",
                new[] { "Invalid user." }));
        }

        if (user.EmailConfirmed)
        {
            return Result<ConfirmEmailResult>.Success(new ConfirmEmailResult(
                true,
                "Your email address is already confirmed. You can log in."));
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

        var identityResult = await _userManager.ConfirmEmailAsync(user, decodedCode);

        // If base64-decoded token failed, try raw code fallback
        if (!identityResult.Succeeded && decodedCode != request.Code)
        {
            identityResult = await _userManager.ConfirmEmailAsync(user, request.Code);
        }

        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors.Select(e => e.Description).ToList();
            _logger.LogWarning("Email confirmation failed for user {UserId}: {Errors}",
                user.Id, string.Join("; ", errors));

            return Result<ConfirmEmailResult>.Success(new ConfirmEmailResult(
                false,
                "Email confirmation failed. The link may have expired or is invalid.",
                errors));
        }

        _logger.LogInformation("Email successfully confirmed for user {UserId} ({Email})", user.Id, user.Email);

        return Result<ConfirmEmailResult>.Success(new ConfirmEmailResult(
            true,
            "Your email address has been confirmed successfully! You can now log in to your account."));
    }
}
