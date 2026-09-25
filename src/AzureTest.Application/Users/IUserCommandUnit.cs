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

public interface IUserCommandUnit
{
    ICommandHandler<LoginUserCommand, LoginUserResult> Login { get; }
    ICommandHandler<LogoutUserCommand, bool> Logout { get; }
    ICommandHandler<RegisterUserCommand, (bool Success, string[] Errors)> Register { get; }
    ICommandHandler<RefreshTokenCommand, RefreshTokenResult> RefreshToken { get; }
    
    // Admin Commands
    ICommandHandler<CreateUserCommand, (bool Success, string[] Errors)> CreateUser { get; }
    ICommandHandler<UpdateUserCommand, (bool Success, string[] Errors)> UpdateUser { get; }
    ICommandHandler<DeleteUserCommand, (bool Success, string[] Errors)> DeleteUser { get; }
    ICommandHandler<ToggleUserLockoutCommand, (bool Success, string[] Errors)> ToggleUserLockout { get; }
    ICommandHandler<CreateRoleCommand, (bool Success, string[] Errors)> CreateRole { get; }
    ICommandHandler<AssignRolesToUserCommand, (bool Success, string[] Errors)> AssignRolesToUser { get; }
}