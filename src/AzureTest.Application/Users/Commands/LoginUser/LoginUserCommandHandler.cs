using AzureTest.Application.Abstractions;


using FluentValidation;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using System.Threading.Tasks;
using System.Threading;

namespace AzureTest.Application.Users.Commands.LoginUser;

public record LoginUserCommand(string Email, string Password) : ICommand<Result<LoginUserResult>>;
public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, Result<LoginUserResult>>
{
    private readonly IUserManagerService _userManagerService;
    public LoginUserCommandHandler(IUserManagerService userManagerService)
    {
        _userManagerService = userManagerService;
    }
    public async Task<Result<LoginUserResult>> HandleAsync(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var passwordValid = await _userManagerService.CheckPasswordAsync(request.Email, request.Password);
        if (!passwordValid)
        {
            return new LoginUserResult(false, null, null, new[] { "Invalid email or password." });
        }
        var tokensDto = await _userManagerService.GenerateJwtTokenAsync(request.Email);
        if (tokensDto.Token == null || tokensDto.RefreshToken == null)
        {
            return new LoginUserResult(false, null, null, new[] { "User not found or token generation failed." });
        }
        return new LoginUserResult(true, tokensDto.Token, tokensDto.RefreshToken, null);
    }
}

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty();
        RuleFor(v => v.Password).NotEmpty();
    }
}
