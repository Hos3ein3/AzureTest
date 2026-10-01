

using FluentValidation;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace AzureTest.Application.Users.Commands.CreateRole;

public sealed record CreateRoleCommand(string RoleName) : ICommand<(bool Success, string[] Errors)>;
internal sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, (bool Success, string[] Errors)>
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<CreateRoleCommandHandler> _logger;
    public CreateRoleCommandHandler(RoleManager<ApplicationRole> roleManager, ILogger<CreateRoleCommandHandler> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }
    public async Task<(bool Success, string[] Errors)> HandleAsync(CreateRoleCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.RoleName))
            return (false, new[] { "Role name cannot be empty." });
        var roleExists = await _roleManager.RoleExistsAsync(command.RoleName);
        if (roleExists)
            return (false, new[] { $"Role '{command.RoleName}' already exists." });
        var role = new ApplicationRole(command.RoleName);
        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            var errors = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(result.Errors, e => e.Description));
            return (false, errors);
        }
        _logger.LogInformation("Admin created role {RoleName}", command.RoleName);
        return (true, Array.Empty<string>());
    }
}

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(v => v.RoleName).NotEmpty();
    }
}
