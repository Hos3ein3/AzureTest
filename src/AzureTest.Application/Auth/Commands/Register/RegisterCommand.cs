using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Services;
using AzureTest.Application.IdentitySettings;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Documents;
using AzureTest.Domain.Entities.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Application.Auth.Commands.Register;

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password,
    string ConfirmPassword
) : ICommand<Result<RegisterCommandResult>>;

public sealed record RegisterCommandResult(
    bool Succeeded,
    ApplicationUser? User,
    bool StayLoginAfterRegister,
    string RedirectUrl,
    IReadOnlyList<string>? Errors
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

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IAuthSessionService sessionService,
        IQueryHandler<GetIdentitySettingQuery, Result<IdentitySetting>> identitySettingQuery)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _identitySettingQuery = identitySettingQuery ?? throw new ArgumentNullException(nameof(identitySettingQuery));
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

        if (setting.StayLoginAfterRegister)
        {
            await _sessionService.CacheUserSessionAsync(user, cancellationToken);
        }

        var redirectUrl = setting.StayLoginAfterRegister
            ? (setting.RedirectAfterLogin ?? "/")
            : (setting.RedirectAfterRegister ?? "/Auth/Login");

        return Result<RegisterCommandResult>.Success(new RegisterCommandResult(
            true, user, setting.StayLoginAfterRegister, redirectUrl, null));
    }
}
