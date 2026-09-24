using AzureTest.Application.Messaging;
using AzureTest.Application.Users.Commands.LoginUser;
using AzureTest.Application.Users.Commands.LogoutUser;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Application.Users.Commands.RegisterUser;

namespace AzureTest.Application.Users;

public interface IUserCommandUnit
{
    ICommandHandler<LoginUserCommand, LoginUserResult> Login { get; }
    ICommandHandler<LogoutUserCommand, bool> Logout { get; }
    ICommandHandler<RegisterUserCommand, (bool Success, string[] Errors)> Register { get; }
    ICommandHandler<RefreshTokenCommand, RefreshTokenResult> RefreshToken { get; }
}