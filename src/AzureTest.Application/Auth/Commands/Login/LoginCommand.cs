using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Services;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace AzureTest.Application.Auth.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    bool RememberMe = true
) : ICommand<Result<LoginCommandResult>>;

public sealed record LoginCommandResult(
    bool Succeeded,
    bool IsLockedOut,
    ApplicationUser? User,
    string? Token,
    string? RefreshToken,
    string? RedirectUrl,
    IReadOnlyList<string>? Errors
);

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginCommandResult>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserManagerService _userManagerService;
    private readonly IAuthSessionService _sessionService;
    private readonly IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> _identitySettingQuery;
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        IUserManagerService userManagerService,
        IAuthSessionService sessionService,
        IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> identitySettingQuery,
        IConfiguration configuration)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _userManagerService = userManagerService ?? throw new ArgumentNullException(nameof(userManagerService));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _identitySettingQuery = identitySettingQuery ?? throw new ArgumentNullException(nameof(identitySettingQuery));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Result<LoginCommandResult>> HandleAsync(LoginCommand request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result<LoginCommandResult>.Success(new LoginCommandResult(
                false, false, null, null, null, null, new[] { "Invalid email or password." }));
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result<LoginCommandResult>.Success(new LoginCommandResult(
                false, true, user, null, null, null, new[] { "Your account is temporarily locked. Please try again later." }));
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return Result<LoginCommandResult>.Success(new LoginCommandResult(
                false, false, null, null, null, null, new[] { "Invalid email or password." }));
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var settingResult = await _identitySettingQuery.HandleAsync(new GetIdentitySettingQuery(), cancellationToken);
        var setting = settingResult.IsSuccess && settingResult.Value != null ? settingResult.Value : new IdentitySetting();

        var enableLogoutOtherDevices = _configuration.GetValue<bool>("FeatureManagement:EnableLogoutOtherDevices", true);

        // Single Session / Logout from other devices logic
        if (enableLogoutOtherDevices && setting.LogoutOtherDevicesOnLogin)
        {
            await _userManager.UpdateSecurityStampAsync(user);
            await _sessionService.InvalidateUserSessionAsync(user.Id, cancellationToken);
        }

        string? token = null;
        string? refreshToken = null;
        var useJwt = _configuration.GetValue<bool>("FeatureManagement:UseJWT", false);

        if (useJwt)
        {
            var tokensDto = await _userManagerService.GenerateJwtTokenAsync(user.Email!);
            token = tokensDto.Token;
            refreshToken = tokensDto.RefreshToken;
        }

        await _sessionService.CacheUserSessionAsync(user, cancellationToken);

        var redirectUrl = !string.IsNullOrWhiteSpace(setting.RedirectAfterLogin) ? setting.RedirectAfterLogin : "/Home/Index";

        return Result<LoginCommandResult>.Success(new LoginCommandResult(
            true, false, user, token, refreshToken, redirectUrl, null));
    }
}
