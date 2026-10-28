using System;
using System.Collections.Generic;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Application.UserPermissions.Queries.GetUserPermissions;
using AzureTest.Contracts.DTOs.Permissions;

namespace AzureTest.Application.UserPermissions;

public class UserPermissionQueryUnit : IUserPermissionQueryUnit
{
    public IQueryHandler<GetUserPermissionsQuery, Result<IReadOnlyList<UserPermissionDto>>> GetUserPermissions { get; }

    public UserPermissionQueryUnit(
        IQueryHandler<GetUserPermissionsQuery, Result<IReadOnlyList<UserPermissionDto>>> getUserPermissions)
    {
        GetUserPermissions = getUserPermissions ?? throw new ArgumentNullException(nameof(getUserPermissions));
    }
}
