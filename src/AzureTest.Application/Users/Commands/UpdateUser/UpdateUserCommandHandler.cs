

using FluentValidation;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AzureTest.Application.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? PhoneNumber
) : ICommand<(bool Success, string[] Errors)>;
internal sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, (bool Success, string[] Errors)>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    public UpdateUserCommandHandler(UserManager<ApplicationUser> userManager, ILogger<UpdateUserCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }
    public async Task<(bool Success, string[] Errors)> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(command.Id.ToString());
        if (user == null)
            return (false, new[] { "User not found." });
        user.FirstName = command.FirstName;
        user.LastName = command.LastName;
        user.PhoneNumber = command.PhoneNumber;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(result.Errors, e => e.Description));
            return (false, errors);
        }
        _logger.LogInformation("Admin updated user {UserId}", command.Id);
        return (true, Array.Empty<string>());
    }
}

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.FirstName).NotEmpty();
        RuleFor(v => v.LastName).NotEmpty();
    }
}
