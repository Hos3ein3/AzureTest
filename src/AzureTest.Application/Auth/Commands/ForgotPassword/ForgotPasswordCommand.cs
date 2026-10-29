using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(
    string Email,
    string? ResetLinkTemplate = null
) : ICommand<Result<ForgotPasswordResult>>;

public sealed record ForgotPasswordResult(
    bool Success,
    string Message,
    string? ResetToken = null,
    string? ResetLink = null
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
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender<ApplicationUser> emailSender,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
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
                "If your email address is registered, instructions to reset your password have been sent to your email address."));
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(resetToken));

        string resetLink;
        if (!string.IsNullOrWhiteSpace(request.ResetLinkTemplate))
        {
            resetLink = request.ResetLinkTemplate
                .Replace("{userId}", user.Id.ToString())
                .Replace("%7BuserId%7D", user.Id.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{code}", Uri.EscapeDataString(encodedToken))
                .Replace("%7Bcode%7D", Uri.EscapeDataString(encodedToken), StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            var scheme = httpContext?.Request.Scheme ?? "https";
            var host = httpContext?.Request.Host.Value ?? "localhost";
            resetLink = $"{scheme}://{host}/Auth/ResetPassword?userId={user.Id}&code={Uri.EscapeDataString(encodedToken)}";
        }

        try
        {
            await _emailSender.SendPasswordResetLinkAsync(user, user.Email!, resetLink);
            _logger.LogInformation("Password reset link dispatched to user {UserId} ({Email})", user.Id, user.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", user.Email);
        }

        return Result<ForgotPasswordResult>.Success(new ForgotPasswordResult(
            true,
            "Instructions to reset your password have been sent to your email address.",
            resetToken,
            resetLink));
    }
}
