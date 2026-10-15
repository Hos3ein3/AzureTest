using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Permissions;

public record PermissionDto(Guid Id, string ControllerName, string ActionName, string DisplayName, string Description, bool IsRestricted, string? AreaName = null);
public record RolePermissionDto(Guid Id, string RoleName, Guid PermissionId, PermissionDto Permission);

public class PermissionFilter : AzureTest.Contracts.DTOs.Common.PagedFilterRequest
{
    public PermissionFilter()
    {
        SortBy = "ControllerName";
        SortDescending = false;
        PageSize = 15;
    }
}

// Queries
public record GetPermissionsQuery(PermissionFilter? Filter = null) : IQuery<AzureTest.Contracts.DTOs.Common.PagedResult<PermissionDto, PermissionFilter>>;
public record GetRolePermissionsQuery(string RoleName) : IQuery<IReadOnlyList<RolePermissionDto>>;

// Handlers
public class GetPermissionsQueryHandler : IQueryHandler<GetPermissionsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<PermissionDto, PermissionFilter>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPermissionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<AzureTest.Contracts.DTOs.Common.PagedResult<PermissionDto, PermissionFilter>> HandleAsync(GetPermissionsQuery query, CancellationToken cancellationToken = default)
    {
        var filter = query.Filter ?? new PermissionFilter();
        var permissions = await _unitOfWork.Permissions.GetAllPermissionsAsync();
        var dtos = permissions.Select(p => new PermissionDto(p.Id, p.ControllerName, p.ActionName, p.DisplayName, p.Description, p.IsRestricted, p.AreaName)).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm;
            dtos = dtos.Where(p => 
                (p.AreaName != null && p.AreaName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (p.ControllerName != null && p.ControllerName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (p.ActionName != null && p.ActionName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (p.DisplayName != null && p.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
            );
        }

        string sortBy = filter.SortBy ?? "ControllerName";
        bool dirAsc = !filter.SortDescending;

        dtos = sortBy switch
        {
            "AreaName" => dirAsc ? dtos.OrderBy(p => p.AreaName).ThenBy(p => p.ControllerName) : dtos.OrderByDescending(p => p.AreaName).ThenByDescending(p => p.ControllerName),
            "ActionName" => dirAsc ? dtos.OrderBy(p => p.ActionName) : dtos.OrderByDescending(p => p.ActionName),
            "DisplayName" => dirAsc ? dtos.OrderBy(p => p.DisplayName) : dtos.OrderByDescending(p => p.DisplayName),
            "IsRestricted" => dirAsc ? dtos.OrderBy(p => p.IsRestricted) : dtos.OrderByDescending(p => p.IsRestricted),
            _ => dirAsc ? dtos.OrderBy(p => p.AreaName).ThenBy(p => p.ControllerName).ThenBy(p => p.ActionName) : dtos.OrderByDescending(p => p.AreaName).ThenByDescending(p => p.ControllerName).ThenByDescending(p => p.ActionName),
        };

        var list = dtos.ToList();
        int totalItems = list.Count;
        int pageSize = filter.PageSize < 1 ? 15 : filter.PageSize;
        int currentPage = filter.PageNumber < 1 ? 1 : filter.PageNumber;

        var items = (filter.PageSize <= 0)
            ? list
            : list.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();

        return new AzureTest.Contracts.DTOs.Common.PagedResult<PermissionDto, PermissionFilter>(items, totalItems, currentPage, pageSize, filter);
    }
}

public class GetRolePermissionsQueryHandler : IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetRolePermissionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<IReadOnlyList<RolePermissionDto>> HandleAsync(GetRolePermissionsQuery query, CancellationToken cancellationToken = default)
    {
        var rolePermissions = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(query.RoleName);
        return rolePermissions.Select(rp => new RolePermissionDto(rp.Id, rp.RoleName, rp.PermissionId, 
            new PermissionDto(rp.Permission.Id, rp.Permission.ControllerName, rp.Permission.ActionName, rp.Permission.DisplayName, rp.Permission.Description, rp.Permission.IsRestricted, rp.Permission.AreaName))).ToList();
    }
}

// Unit
public interface IPermissionQueryUnit
{
    IQueryHandler<GetPermissionsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<PermissionDto, PermissionFilter>> GetPermissions { get; }
    IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>> GetRolePermissions { get; }
}

public class PermissionQueryUnit : IPermissionQueryUnit
{
    public IQueryHandler<GetPermissionsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<PermissionDto, PermissionFilter>> GetPermissions { get; }
    public IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>> GetRolePermissions { get; }
    
    public PermissionQueryUnit(
        IQueryHandler<GetPermissionsQuery, AzureTest.Contracts.DTOs.Common.PagedResult<PermissionDto, PermissionFilter>> getPermissions,
        IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>> getRolePermissions)
    {
        GetPermissions = getPermissions;
        GetRolePermissions = getRolePermissions;
    }
}
