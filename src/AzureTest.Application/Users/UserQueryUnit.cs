using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Users.Queries.GetAllRoles;
using AzureTest.Application.Users.Queries.GetAllUsers;
using AzureTest.Application.Users.Queries.GetUserById;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Users;

namespace AzureTest.Application.Users;

public class UserQueryUnit : IUserQueryUnit
{
    public IQueryHandler<GetAllUsersQuery, PagedResult<UserListDto>> GetAllUsers { get; }
    public IQueryHandler<GetUserByIdQuery, UserDto?> GetUserById { get; }
    public IQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>> GetAllRoles { get; }

    public UserQueryUnit(
        IQueryHandler<GetAllUsersQuery, PagedResult<UserListDto>> getAllUsers,
        IQueryHandler<GetUserByIdQuery, UserDto?> getUserById,
        IQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>> getAllRoles)
    {
        GetAllUsers = getAllUsers;
        GetUserById = getUserById;
        GetAllRoles = getAllRoles;
    }
}