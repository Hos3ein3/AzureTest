using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Users.Queries.GetAllUsers;

public sealed record GetAllUsersQuery(
    string? SearchTerm = null,
    string? Role = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 10,
    AzureTest.Contracts.DTOs.Users.GetAllUsersFilters? Filters = null
) : IQuery<PagedResult<UserListDto>>
{
    public GetAllUsersQuery(AzureTest.Contracts.DTOs.Users.GetAllUsersFilters filters)
        : this(filters.SearchTerm, filters.Role, filters.SortBy, filters.SortDescending, filters.PageNumber, filters.PageSize, filters)
    {
    }
}

internal sealed class GetAllUsersQueryHandler : BaseQueryHandler<GetAllUsersQuery, PagedResult<UserListDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetAllUsersQueryHandler(UserManager<ApplicationUser> userManager, ILogger<GetAllUsersQueryHandler> logger)
        : base(logger)
    {
        _userManager = userManager;
    }

    protected override async Task<PagedResult<UserListDto>> ExecuteAsync(GetAllUsersQuery query, CancellationToken cancellationToken)
    {
        var allUsers = _userManager.Users.ToList();

        // Filter
        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.ToLower();
            allUsers = allUsers.Where(u =>
                (u.Email?.ToLower().Contains(term) ?? false) ||
                u.FirstName.ToLower().Contains(term) ||
                u.LastName.ToLower().Contains(term)).ToList();
        }

        // Build DTOs with roles
        var dtos = new List<UserListDto>();
        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!string.IsNullOrWhiteSpace(query.Role) &&
                !roles.Contains(query.Role, StringComparer.OrdinalIgnoreCase))
                continue;

            dtos.Add(new UserListDto(
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                user.EmailConfirmed,
                user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                roles
            ));
        }

        // Sort
        dtos = query.SortBy switch
        {
            "Email" => query.SortDescending ? dtos.OrderByDescending(u => u.Email).ToList() : dtos.OrderBy(u => u.Email).ToList(),
            "Roles" => query.SortDescending ? dtos.OrderByDescending(u => string.Join(",", u.Roles)).ToList() : dtos.OrderBy(u => string.Join(",", u.Roles)).ToList(),
            _ => query.SortDescending ? dtos.OrderByDescending(u => u.FullName).ToList() : dtos.OrderBy(u => u.FullName).ToList(),
        };

        // Paginate
        var totalCount = dtos.Count;
        var items = dtos.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return new PagedResult<UserListDto>(items, totalCount, query.PageNumber, query.PageSize);
    }
}
