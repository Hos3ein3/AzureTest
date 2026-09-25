using System.Collections.Generic;
using AzureTest.Application.Messaging;
using AzureTest.Application.Users.Queries.GetAllRoles;
using AzureTest.Application.Users.Queries.GetAllUsers;
using AzureTest.Application.Users.Queries.GetUserById;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Users;

namespace AzureTest.Application.Users;

public interface IUserQueryUnit
{
    IQueryHandler<GetAllUsersQuery, PagedResult<UserListDto>> GetAllUsers { get; }
    IQueryHandler<GetUserByIdQuery, UserDto?> GetUserById { get; }
    IQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>> GetAllRoles { get; }
}