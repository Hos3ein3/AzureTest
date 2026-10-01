

using FluentValidation;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AzureTest.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string[] Roles
) : ICommand<(bool Success, string[] Errors)>;
internal sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, (bool Success, string[] Errors)>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CreateUserCommandHandler> _logger;
    public CreateUserCommandHandler(UserManager<ApplicationUser> userManager, ILogger<CreateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }
    public async Task<(bool Success, string[] Errors)> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = command.Email,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = true // Automatically confirm for admin-created users
        };
        var result = await _userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(result.Errors, e => e.Description));
            return (false, errors);
        }
        if (command.Roles != null && command.Roles.Length > 0)
        {
            await _userManager.AddToRolesAsync(user, command.Roles);
        }
        _logger.LogInformation("Admin created user {Email}", command.Email);
        return (true, Array.Empty<string>());
    }
}

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty();
        RuleFor(v => v.FirstName).NotEmpty();
        RuleFor(v => v.LastName).NotEmpty();
        RuleFor(v => v.Password).NotEmpty();
    }
}
