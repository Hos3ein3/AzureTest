using AzureTest.Application.Messaging;
using AzureTest.Application.Users.Commands.LoginUser;
using AzureTest.Application.Users.Commands.LogoutUser;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Application.Users.Commands.RegisterUser;

namespace AzureTest.Application.Users;

public class UserCommandUnit : IUserCommandUnit
{
    public ICommandHandler<LoginUserCommand, LoginUserResult> Login { get; }
    public ICommandHandler<LogoutUserCommand, bool> Logout { get; }
    public ICommandHandler<RegisterUserCommand, (bool Success, string[] Errors)> Register { get; }
    public ICommandHandler<RefreshTokenCommand, RefreshTokenResult> RefreshToken { get; }

    public UserCommandUnit(
        ICommandHandler<LoginUserCommand, LoginUserResult> login,
        ICommandHandler<LogoutUserCommand, bool> logout,
        ICommandHandler<RegisterUserCommand, (bool Success, string[] Errors)> register,
        ICommandHandler<RefreshTokenCommand, RefreshTokenResult> refreshToken)
    {
        Login = login;
        Logout = logout;
        Register = register;
        RefreshToken = refreshToken;
    }
}