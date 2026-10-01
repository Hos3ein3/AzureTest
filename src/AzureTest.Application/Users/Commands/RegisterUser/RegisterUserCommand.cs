using AzureTest.Application.Abstractions;


using FluentValidation;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace AzureTest.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(string Email, string Password, string FirstName, string LastName) : ICommand;
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand>
{
    private readonly IUserManagerService _userManagerService;
    public RegisterUserCommandHandler(IUserManagerService userManagerService)
    {
        _userManagerService = userManagerService;
    }
    public async Task<Result> HandleAsync(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var res = await _userManagerService.RegisterUserAsync(request.Email, request.Password, request.FirstName, request.LastName);
        if (res.Success) return Result.Success();
        return Result.Failure(string.Join(", ", res.Errors));
    }
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty();
        RuleFor(v => v.Password).NotEmpty();
        RuleFor(v => v.FirstName).NotEmpty();
        RuleFor(v => v.LastName).NotEmpty();
    }
}
