using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Users.Queries.GetAllRoles;

public sealed record GetAllRolesQuery() : IQuery<IReadOnlyList<RoleDto>>;

internal sealed class GetAllRolesQueryHandler : BaseQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public GetAllRolesQueryHandler(RoleManager<ApplicationRole> roleManager, ILogger<GetAllRolesQueryHandler> logger)
        : base(logger)
    {
        _roleManager = roleManager;
    }

    protected override async Task<IReadOnlyList<RoleDto>> ExecuteAsync(GetAllRolesQuery query, CancellationToken cancellationToken)
    {
        var roles = await _roleManager.Roles.ToListAsync(cancellationToken);
        return roles.Select(r => new RoleDto(r.Id, r.Name ?? string.Empty)).ToList();
    }
}
