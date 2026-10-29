
using System.Text;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Services;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password,
    string ConfirmPassword,
    string? ConfirmationLinkTemplate = null
) : ICommand<Result<RegisterCommandResult>>;

public sealed record RegisterCommandResult(
    bool Succeeded,
    ApplicationUser? User,
    bool StayLoginAfterRegister,
    string RedirectUrl,
    IReadOnlyList<string>? Errors,
    bool ConfirmationEmailSent = false
);

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match.");
    }
}

public sealed class RegisterCommandHandler : ICommandHandler<RegisterCommand, Result<RegisterCommandResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthSessionService _sessionService;
    private readonly IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> _identitySettingQuery;
    private readonly IEmailSender<ApplicationUser> _emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IAuthSessionService sessionService,
        IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> identitySettingQuery,
        IEmailSender<ApplicationUser> emailSender,
        IHttpContextAccessor httpContextAccessor,
        ILogger<RegisterCommandHandler> logger)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _identitySettingQuery = identitySettingQuery ?? throw new ArgumentNullException(nameof(identitySettingQuery));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<RegisterCommandResult>> HandleAsync(RegisterCommand request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result<RegisterCommandResult>.Success(new RegisterCommandResult(
                false, null, false, "/Auth/Register", new[] { "An account with this email already exists." }));
        }

        var nameParts = (request.Name ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = firstName,
            LastName = lastName
        };

        var creationResult = await _userManager.CreateAsync(user, request.Password);
        if (!creationResult.Succeeded)
        {
            var errors = creationResult.Errors.Select(e => e.Description).ToList();
            return Result<RegisterCommandResult>.Success(new RegisterCommandResult(
                false, null, false, "/Auth/Register", errors));
        }

        var settingResult = await _identitySettingQuery.HandleAsync(new GetIdentitySettingQuery(), cancellationToken);
        var setting = settingResult.IsSuccess && settingResult.Value != null ? settingResult.Value : new IdentitySetting();

        if (setting.RolesForRegister != null && setting.RolesForRegister.Any())
        {
            foreach (var role in setting.RolesForRegister)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
        }

        var confirmationEmailSent = false;
        if (!user.EmailConfirmed)
        {
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                string confirmationLink;
                if (!string.IsNullOrWhiteSpace(request.ConfirmationLinkTemplate))
                {
                    confirmationLink = request.ConfirmationLinkTemplate
                        .Replace("{userId}", user.Id.ToString())
                        .Replace("{code}", Uri.EscapeDataString(encodedToken));
                }
                else
                {
                    var httpContext = _httpContextAccessor?.HttpContext;
                    var scheme = httpContext?.Request.Scheme ?? "https";
                    var host = httpContext?.Request.Host.Value ?? "localhost";
                    confirmationLink = $"{scheme}://{host}/Auth/ConfirmEmail?userId={user.Id}&code={Uri.EscapeDataString(encodedToken)}";
                }

                await _emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmationLink);
                confirmationEmailSent = true;
                _logger.LogInformation("Email confirmation link dispatched to user {UserId} ({Email})", user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email}", user.Email);
            }
        }

        if (setting.StayLoginAfterRegister)
        {
            await _sessionService.CacheUserSessionAsync(user, cancellationToken);
        }

        var redirectUrl = setting.StayLoginAfterRegister
            ? (setting.RedirectAfterLogin ?? "/")
            : (setting.RedirectAfterRegister ?? "/Auth/Login");

        return Result<RegisterCommandResult>.Success(new RegisterCommandResult(
            true, user, setting.StayLoginAfterRegister, redirectUrl, null, confirmationEmailSent));
    }
}
