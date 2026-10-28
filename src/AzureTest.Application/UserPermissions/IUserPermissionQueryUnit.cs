using System.Collections.Generic;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPermissions.Queries.GetUserPermissions;
using AzureTest.Contracts.DTOs.Permissions;

namespace AzureTest.Application.UserPermissions;

public interface IUserPermissionQueryUnit
{
    IQueryHandler<GetUserPermissionsQuery, Result<IReadOnlyList<UserPermissionDto>>> GetUserPermissions { get; }
}
