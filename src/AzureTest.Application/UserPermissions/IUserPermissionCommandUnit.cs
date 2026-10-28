using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPermissions.Commands.UpdateUserPermissions;

namespace AzureTest.Application.UserPermissions;

public interface IUserPermissionCommandUnit
{
    ICommandHandler<UpdateUserPermissionsCommand> UpdateUserPermissions { get; }
}
