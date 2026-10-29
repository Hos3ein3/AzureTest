using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Commands.ChangePassword;
using AzureTest.Application.Auth.Commands.ForgotPassword;
using AzureTest.Application.Auth.Commands.GoogleCallback;
using AzureTest.Application.Auth.Commands.Login;
using AzureTest.Application.Auth.Commands.Logout;
using AzureTest.Application.Auth.Commands.Register;
using AzureTest.Application.Messaging;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Contracts.DTOs.Users;

namespace AzureTest.Application.Auth;

public interface IAuthCommandUnit
{
    ICommandHandler<LoginCommand, Result<LoginCommandResult>> Login { get; }
    ICommandHandler<RegisterCommand, Result<RegisterCommandResult>> Register { get; }
    ICommandHandler<LogoutCommand, Result> Logout { get; }
    ICommandHandler<ProcessGoogleCallbackCommand, Result<GoogleCallbackResult>> GoogleCallback { get; }
    ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>> RefreshToken { get; }
    ICommandHandler<ForgotPasswordCommand, Result<ForgotPasswordResult>> ForgotPassword { get; }
    ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResult>> ChangePassword { get; }
}
