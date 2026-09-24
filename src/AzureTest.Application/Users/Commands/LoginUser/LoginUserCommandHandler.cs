using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Users.Commands.LoginUser;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, LoginUserResult>
{
    private readonly IUserManagerService _userManagerService;

    public LoginUserCommandHandler(IUserManagerService userManagerService)
    {
        _userManagerService = userManagerService;
    }

    public async Task<LoginUserResult> HandleAsync(LoginUserCommand request, CancellationToken cancellationToken)
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
