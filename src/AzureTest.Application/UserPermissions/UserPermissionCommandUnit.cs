using System;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPermissions.Commands.UpdateUserPermissions;

namespace AzureTest.Application.UserPermissions;

public class UserPermissionCommandUnit : IUserPermissionCommandUnit
{
    public ICommandHandler<UpdateUserPermissionsCommand> UpdateUserPermissions { get; }

    public UserPermissionCommandUnit(
        ICommandHandler<UpdateUserPermissionsCommand> updateUserPermissions)
    {
        UpdateUserPermissions = updateUserPermissions ?? throw new ArgumentNullException(nameof(updateUserPermissions));
    }
}
