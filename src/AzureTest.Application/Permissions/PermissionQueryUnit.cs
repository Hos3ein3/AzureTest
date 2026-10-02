using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Permissions;

public record PermissionDto(Guid Id, string ControllerName, string ActionName, string DisplayName, string Description, bool IsRestricted);
public record RolePermissionDto(Guid Id, string RoleName, Guid PermissionId, PermissionDto Permission);

// Queries
public record GetPermissionsQuery : IQuery<IReadOnlyList<PermissionDto>>;
public record GetRolePermissionsQuery(string RoleName) : IQuery<IReadOnlyList<RolePermissionDto>>;

// Handlers
public class GetPermissionsQueryHandler : IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetPermissionsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    public async Task<IReadOnlyList<PermissionDto>> HandleAsync(GetPermissionsQuery query, CancellationToken cancellationToken = default)
    {
        var permissions = await _unitOfWork.Permissions.GetAllPermissionsAsync();
        return permissions.Select(p => new PermissionDto(p.Id, p.ControllerName, p.ActionName, p.DisplayName, p.Description, p.IsRestricted)).ToList();
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
            new PermissionDto(rp.Permission.Id, rp.Permission.ControllerName, rp.Permission.ActionName, rp.Permission.DisplayName, rp.Permission.Description, rp.Permission.IsRestricted))).ToList();
    }
}

// Unit
public interface IPermissionQueryUnit
{
    IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>> GetPermissions { get; }
    IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>> GetRolePermissions { get; }
}

public class PermissionQueryUnit : IPermissionQueryUnit
{
    public IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>> GetPermissions { get; }
    public IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>> GetRolePermissions { get; }
    
    public PermissionQueryUnit(
        IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>> getPermissions,
        IQueryHandler<GetRolePermissionsQuery, IReadOnlyList<RolePermissionDto>> getRolePermissions)
    {
        GetPermissions = getPermissions;
        GetRolePermissions = getRolePermissions;
    }
}
