using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Commands.GoogleCallback;
using AzureTest.Application.Auth.Commands.Login;
using AzureTest.Application.Auth.Commands.Logout;
using AzureTest.Application.Auth.Commands.Register;
using AzureTest.Application.Messaging;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Contracts.DTOs.Users;

namespace AzureTest.Application.Auth;

public class AuthCommandUnit : IAuthCommandUnit
{
    public ICommandHandler<LoginCommand, Result<LoginCommandResult>> Login { get; }
    public ICommandHandler<RegisterCommand, Result<RegisterCommandResult>> Register { get; }
    public ICommandHandler<LogoutCommand, Result> Logout { get; }
    public ICommandHandler<ProcessGoogleCallbackCommand, Result<GoogleCallbackResult>> GoogleCallback { get; }
    public ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>> RefreshToken { get; }

    public AuthCommandUnit(
        ICommandHandler<LoginCommand, Result<LoginCommandResult>> login,
        ICommandHandler<RegisterCommand, Result<RegisterCommandResult>> register,
        ICommandHandler<LogoutCommand, Result> logout,
        ICommandHandler<ProcessGoogleCallbackCommand, Result<GoogleCallbackResult>> googleCallback,
        ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>> refreshToken)
    {
        Login = login ?? throw new ArgumentNullException(nameof(login));
        Register = register ?? throw new ArgumentNullException(nameof(register));
        Logout = logout ?? throw new ArgumentNullException(nameof(logout));
        GoogleCallback = googleCallback ?? throw new ArgumentNullException(nameof(googleCallback));
        RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
    }
}
