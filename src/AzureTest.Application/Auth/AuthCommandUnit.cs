using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Auth.Commands.ChangePassword;
using AzureTest.Application.Auth.Commands.ConfirmEmail;
using AzureTest.Application.Auth.Commands.ForgotPassword;
using AzureTest.Application.Auth.Commands.GoogleCallback;
using AzureTest.Application.Auth.Commands.Login;
using AzureTest.Application.Auth.Commands.Logout;
using AzureTest.Application.Auth.Commands.Register;
using AzureTest.Application.Auth.Commands.ResetPassword;
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
    public ICommandHandler<ForgotPasswordCommand, Result<ForgotPasswordResult>> ForgotPassword { get; }
    public ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResult>> ChangePassword { get; }
    public ICommandHandler<ConfirmEmailCommand, Result<ConfirmEmailResult>> ConfirmEmail { get; }
    public ICommandHandler<ResetPasswordCommand, Result<ResetPasswordResult>> ResetPassword { get; }

    public AuthCommandUnit(
        ICommandHandler<LoginCommand, Result<LoginCommandResult>> login,
        ICommandHandler<RegisterCommand, Result<RegisterCommandResult>> register,
        ICommandHandler<LogoutCommand, Result> logout,
        ICommandHandler<ProcessGoogleCallbackCommand, Result<GoogleCallbackResult>> googleCallback,
        ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>> refreshToken,
        ICommandHandler<ForgotPasswordCommand, Result<ForgotPasswordResult>> forgotPassword,
        ICommandHandler<ChangePasswordCommand, Result<ChangePasswordResult>> changePassword,
        ICommandHandler<ConfirmEmailCommand, Result<ConfirmEmailResult>> confirmEmail,
        ICommandHandler<ResetPasswordCommand, Result<ResetPasswordResult>> resetPassword)
    {
        Login = login ?? throw new ArgumentNullException(nameof(login));
        Register = register ?? throw new ArgumentNullException(nameof(register));
        Logout = logout ?? throw new ArgumentNullException(nameof(logout));
        GoogleCallback = googleCallback ?? throw new ArgumentNullException(nameof(googleCallback));
        RefreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
        ForgotPassword = forgotPassword ?? throw new ArgumentNullException(nameof(forgotPassword));
        ChangePassword = changePassword ?? throw new ArgumentNullException(nameof(changePassword));
        ConfirmEmail = confirmEmail ?? throw new ArgumentNullException(nameof(confirmEmail));
        ResetPassword = resetPassword ?? throw new ArgumentNullException(nameof(resetPassword));
    }
}
