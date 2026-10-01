using AzureTest.Application.Abstractions;


using AzureTest.Contracts.DTOs.Users;
using AzureTest.Application.Messaging;
using AzureTest.Application.Users.Commands.LoginUser;
using AzureTest.Application.Users.Commands.LogoutUser;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Application.Users.Commands.RegisterUser;
using AzureTest.Application.Users.Commands.CreateUser;
using AzureTest.Application.Users.Commands.UpdateUser;
using AzureTest.Application.Users.Commands.DeleteUser;
using AzureTest.Application.Users.Commands.ToggleUserLockout;
using AzureTest.Application.Users.Commands.CreateRole;
using AzureTest.Application.Users.Commands.AssignRolesToUser;
using System;

namespace AzureTest.Application.Users;

public class UserCommandUnit : IUserCommandUnit
{
    public ICommandHandler<LoginUserCommand, Result<LoginUserResult>> Login { get; }
    public ICommandHandler<LogoutUserCommand, Result<bool>> Logout { get; }
    public ICommandHandler<RegisterUserCommand> Register { get; }
    public ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>> RefreshToken { get; }
    public ICommandHandler<CreateUserCommand, (bool Success, string[] Errors)> CreateUser { get; }
    public ICommandHandler<UpdateUserCommand, (bool Success, string[] Errors)> UpdateUser { get; }
    public ICommandHandler<DeleteUserCommand, (bool Success, string[] Errors)> DeleteUser { get; }
    public ICommandHandler<ToggleUserLockoutCommand, (bool Success, string[] Errors)> ToggleUserLockout { get; }
    public ICommandHandler<CreateRoleCommand, (bool Success, string[] Errors)> CreateRole { get; }
    public ICommandHandler<AssignRolesToUserCommand, (bool Success, string[] Errors)> AssignRolesToUser { get; }
    public UserCommandUnit(
        ICommandHandler<LoginUserCommand, Result<LoginUserResult>> login,
        ICommandHandler<LogoutUserCommand, Result<bool>> logout,
        ICommandHandler<RegisterUserCommand> register,
        ICommandHandler<RefreshTokenCommand, Result<RefreshTokenResult>> refreshToken,
        ICommandHandler<CreateUserCommand, (bool Success, string[] Errors)> createUser,
        ICommandHandler<UpdateUserCommand, (bool Success, string[] Errors)> updateUser,
        ICommandHandler<DeleteUserCommand, (bool Success, string[] Errors)> deleteUser,
        ICommandHandler<ToggleUserLockoutCommand, (bool Success, string[] Errors)> toggleUserLockout,
        ICommandHandler<CreateRoleCommand, (bool Success, string[] Errors)> createRole,
        ICommandHandler<AssignRolesToUserCommand, (bool Success, string[] Errors)> assignRolesToUser)
    {
        Login = login;
        Logout = logout;
        Register = register;
        RefreshToken = refreshToken;
        CreateUser = createUser;
        UpdateUser = updateUser;
        DeleteUser = deleteUser;
        ToggleUserLockout = toggleUserLockout;
        CreateRole = createRole;
        AssignRolesToUser = assignRolesToUser;
    }
}