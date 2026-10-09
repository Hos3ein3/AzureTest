using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Permissions;
using AzureTest.Domain.Entities;
using AzureTest.Application.Messaging;

namespace AzureTest.Application.Permissions;


public record CreatePermissionCommand(string ControllerName, string ActionName, string DisplayName, string? Description) : ICommand;

public class CreatePermissionCommandHandler : ICommandHandler<CreatePermissionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public CreatePermissionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(CreatePermissionCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Permissions.GetAllPermissionsAsync();
        var match = existing.FirstOrDefault(p => p.ControllerName == command.ControllerName && p.ActionName == command.ActionName);
        if (match != null)
        {
            return Result.Failure("Permission already exists for this Controller/Action.");
        }
        
        _unitOfWork.Permissions.Add(new Permission
        {
            ControllerName = command.ControllerName,
            ActionName = command.ActionName,
            DisplayName = command.DisplayName,
            Description = command.Description,
            IsRestricted = true // Default manual to restricted
        });
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public record SyncPermissionsCommand(List<DiscoveredPermissionDto> DiscoveredPermissions) : ICommand;
public record TogglePermissionRestrictionCommand(Guid PermissionId, bool IsRestricted) : ICommand;

public record RolePermissionsChangedEvent(string RoleName);

public record AssignRolePermissionCommand(string RoleName, Guid PermissionId) : ICommand;
public record RemoveRolePermissionCommand(string RoleName, Guid PermissionId) : ICommand;

public class SyncPermissionsCommandHandler : ICommandHandler<SyncPermissionsCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public SyncPermissionsCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(SyncPermissionsCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Permissions.GetAllPermissionsAsync();
        foreach (var discovered in command.DiscoveredPermissions)
        {
            var match = existing.FirstOrDefault(p => p.ControllerName == discovered.ControllerName && p.ActionName == discovered.ActionName);
            if (match == null)
            {
                _unitOfWork.Permissions.Add(new Permission
                {
                    ControllerName = discovered.ControllerName,
                    ActionName = discovered.ActionName,
                    DisplayName = discovered.DisplayName,
                    Description = discovered.Description,
                    IsRestricted = true
                });
            }
            else
            {
                match.DisplayName = discovered.DisplayName;
                match.Description = discovered.Description;
                _unitOfWork.Permissions.Update(match);
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class TogglePermissionRestrictionCommandHandler : ICommandHandler<TogglePermissionRestrictionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    public TogglePermissionRestrictionCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> HandleAsync(TogglePermissionRestrictionCommand command, CancellationToken cancellationToken = default)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(command.PermissionId);
        if (permission == null) return Result.Failure("Permission not found");

        permission.IsRestricted = command.IsRestricted;
        _unitOfWork.Permissions.Update(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

public class AssignRolePermissionCommandHandler : ICommandHandler<AssignRolePermissionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventDispatcher _eventDispatcher;
    public AssignRolePermissionCommandHandler(IUnitOfWork unitOfWork, IEventDispatcher publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _eventDispatcher = publishEndpoint;
    }

    public async Task<Result> HandleAsync(AssignRolePermissionCommand command, CancellationToken cancellationToken = default)
    {
        var permission = await _unitOfWork.Permissions.GetByIdAsync(command.PermissionId);
        if (permission == null) return Result.Failure("Permission not found");

        var existing = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(command.RoleName);
        if (existing.Any(rp => rp.PermissionId == command.PermissionId)) return Result.Success();

        _unitOfWork.RolePermissions.Add(new RolePermission
        {
            RoleName = command.RoleName,
            PermissionId = command.PermissionId
        });
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        await _eventDispatcher.PublishAsync(new RolePermissionsChangedEvent(command.RoleName), cancellationToken);
        
        return Result.Success();
    }
}

public class RemoveRolePermissionCommandHandler : ICommandHandler<RemoveRolePermissionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventDispatcher _eventDispatcher;
    public RemoveRolePermissionCommandHandler(IUnitOfWork unitOfWork, IEventDispatcher publishEndpoint)
    {
        _unitOfWork = unitOfWork;
        _eventDispatcher = publishEndpoint;
    }

    public async Task<Result> HandleAsync(RemoveRolePermissionCommand command, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(command.RoleName);
        var match = existing.FirstOrDefault(rp => rp.PermissionId == command.PermissionId);
        if (match != null)
        {
            _unitOfWork.RolePermissions.Remove(match);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _eventDispatcher.PublishAsync(new RolePermissionsChangedEvent(command.RoleName), cancellationToken);
        }
        return Result.Success();
    }
}

public interface IPermissionCommandUnit
{
    ICommandHandler<CreatePermissionCommand> CreatePermission { get; }
    public ICommandHandler<SyncPermissionsCommand> SyncPermissions { get; }
    ICommandHandler<TogglePermissionRestrictionCommand> TogglePermissionRestriction { get; }
    ICommandHandler<AssignRolePermissionCommand> AssignRolePermission { get; }
    ICommandHandler<RemoveRolePermissionCommand> RemoveRolePermission { get; }
}

public class PermissionCommandUnit : IPermissionCommandUnit
{
    public ICommandHandler<CreatePermissionCommand> CreatePermission { get; }
    public ICommandHandler<SyncPermissionsCommand> SyncPermissions { get; }
    public ICommandHandler<TogglePermissionRestrictionCommand> TogglePermissionRestriction { get; }
    public ICommandHandler<AssignRolePermissionCommand> AssignRolePermission { get; }
    public ICommandHandler<RemoveRolePermissionCommand> RemoveRolePermission { get; }

    public PermissionCommandUnit(
        ICommandHandler<CreatePermissionCommand> createPermission,
        ICommandHandler<SyncPermissionsCommand> syncPermissions,
        ICommandHandler<TogglePermissionRestrictionCommand> togglePermissionRestriction,
        ICommandHandler<AssignRolePermissionCommand> assignRolePermission,
        ICommandHandler<RemoveRolePermissionCommand> removeRolePermission)
    {
        CreatePermission = createPermission;
        SyncPermissions = syncPermissions;
        TogglePermissionRestriction = togglePermissionRestriction;
        AssignRolePermission = assignRolePermission;
        RemoveRolePermission = removeRolePermission;
    }
}
