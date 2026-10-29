using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Services;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AzureTest.Application.Auth.Commands.GoogleCallback;

public sealed record ProcessGoogleCallbackCommand(
    string LoginProvider,
    string ProviderKey,
    string? Email,
    string? GivenName,
    string? Surname,
    GoogleAuthMode Mode,
    string? ReturnUrl
) : ICommand<Result<GoogleCallbackResult>>;

public sealed record GoogleCallbackResult(
    bool Succeeded,
    bool IsLockedOut,
    ApplicationUser? User,
    string? RedirectUrl,
    string? ErrorMessage,
    string? FallbackAction,
    DateTimeOffset? LockoutEnd = null
);

public sealed class ProcessGoogleCallbackCommandValidator : AbstractValidator<ProcessGoogleCallbackCommand>
{
    public ProcessGoogleCallbackCommandValidator()
    {
        RuleFor(x => x.LoginProvider).NotEmpty().WithMessage("LoginProvider is required.");
        RuleFor(x => x.ProviderKey).NotEmpty().WithMessage("ProviderKey is required.");
    }
}

public sealed class ProcessGoogleCallbackCommandHandler : ICommandHandler<ProcessGoogleCallbackCommand, Result<GoogleCallbackResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthSessionService _sessionService;
    private readonly IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> _identitySettingQuery;
    private readonly IConfiguration _configuration;

    public ProcessGoogleCallbackCommandHandler(
        UserManager<ApplicationUser> userManager,
        IAuthSessionService sessionService,
        IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> identitySettingQuery,
        IConfiguration configuration)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _identitySettingQuery = identitySettingQuery ?? throw new ArgumentNullException(nameof(identitySettingQuery));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Result<GoogleCallbackResult>> HandleAsync(
        ProcessGoogleCallbackCommand request,
        CancellationToken cancellationToken = default)
    {
        var settingResult = await _identitySettingQuery.HandleAsync(new GetIdentitySettingQuery(), cancellationToken);
        var setting = settingResult.IsSuccess && settingResult.Value != null ? settingResult.Value : new IdentitySetting();
        var defaultRedirect = !string.IsNullOrWhiteSpace(setting.RedirectAfterLogin) ? setting.RedirectAfterLogin : "/Home/Index";
        var enableLogoutOtherDevices = _configuration.GetValue<bool>("FeatureManagement:EnableLogoutOtherDevices", true);

        // Case 1: Account already linked via external login
        var linkedUser = await _userManager.FindByLoginAsync(request.LoginProvider, request.ProviderKey);
        if (linkedUser != null)
        {
            if (request.Mode == GoogleAuthMode.RegisterOnly)
            {
                return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                    false, false, null, null, "This Google account is already registered. Please sign in.", "Login"));
            }

            if (await _userManager.IsLockedOutAsync(linkedUser))
            {
                return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                    false, true, linkedUser, null, "User account is locked.", null, linkedUser.LockoutEnd));
            }

            if (enableLogoutOtherDevices && setting.LogoutOtherDevicesOnLogin)
            {
                await _userManager.UpdateSecurityStampAsync(linkedUser);
                await _sessionService.InvalidateUserSessionAsync(linkedUser.Id, cancellationToken);
            }

            await _sessionService.CacheUserSessionAsync(linkedUser, cancellationToken);

            var redirectTarget = !string.IsNullOrWhiteSpace(request.ReturnUrl) ? request.ReturnUrl : defaultRedirect;
            return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                true, false, linkedUser, redirectTarget, null, null));
        }

        // Case 2: Not linked yet, check by email
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                false, false, null, null, "Google did not return an email address.", null));
        }

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null && request.Mode == GoogleAuthMode.SignInOnly)
        {
            return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                false, false, null, null, "No account exists for this email. Please register first.", "Register"));
        }

        if (user != null && request.Mode == GoogleAuthMode.RegisterOnly)
        {
            return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                false, false, null, null, "This email is already registered. Please sign in.", "Login"));
        }

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true,
                FirstName = request.GivenName ?? string.Empty,
                LastName = request.Surname ?? string.Empty
            };

            var creationResult = await _userManager.CreateAsync(user);
            if (!creationResult.Succeeded)
            {
                var errorMsg = string.Join(" ", creationResult.Errors.Select(e => e.Description));
                return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                    false, false, null, null, errorMsg, null));
            }

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
        }
        else if (!user.EmailConfirmed)
        {
            await _userManager.RemovePasswordAsync(user);
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        var addLoginResult = await _userManager.AddLoginAsync(
            user, new UserLoginInfo(request.LoginProvider, request.ProviderKey, request.LoginProvider));

        if (!addLoginResult.Succeeded)
        {
            var errorMsg = string.Join(" ", addLoginResult.Errors.Select(e => e.Description));
            return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
                false, false, null, null, errorMsg, null));
        }

        if (enableLogoutOtherDevices && setting.LogoutOtherDevicesOnLogin)
        {
            await _userManager.UpdateSecurityStampAsync(user);
            await _sessionService.InvalidateUserSessionAsync(user.Id, cancellationToken);
        }

        await _sessionService.CacheUserSessionAsync(user, cancellationToken);

        var finalRedirect = !string.IsNullOrWhiteSpace(request.ReturnUrl) ? request.ReturnUrl : defaultRedirect;
        return Result<GoogleCallbackResult>.Success(new GoogleCallbackResult(
            true, false, user, finalRedirect, null, null));
    }
}
