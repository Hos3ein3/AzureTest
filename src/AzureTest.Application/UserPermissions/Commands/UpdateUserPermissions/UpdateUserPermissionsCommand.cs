using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Domain.Entities;
using FluentValidation;

namespace AzureTest.Application.UserPermissions.Commands.UpdateUserPermissions;

public record UpdateUserPermissionsCommand(Guid UserId, List<Guid> SelectedPermissionIds) : ICommand;

public class UpdateUserPermissionsCommandValidator : AbstractValidator<UpdateUserPermissionsCommand>
{
    public UpdateUserPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.SelectedPermissionIds)
            .NotNull().WithMessage("SelectedPermissionIds collection cannot be null.");
    }
}

public class UpdateUserPermissionsCommandHandler : ICommandHandler<UpdateUserPermissionsCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRedisService? _redisService;

    public UpdateUserPermissionsCommandHandler(IUnitOfWork unitOfWork, IRedisService? redisService = null)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _redisService = redisService;
    }

    public async Task<Result> HandleAsync(UpdateUserPermissionsCommand command, CancellationToken cancellationToken = default)
    {
        if (command.UserId == Guid.Empty)
        {
            return Result.Failure("User ID cannot be empty.");
        }

        var selectedIds = (command.SelectedPermissionIds ?? new List<Guid>()).Distinct().ToHashSet();
        var existing = await _unitOfWork.UserPermissions.GetPermissionsByUserIdAsync(command.UserId, cancellationToken);
        var existingMap = existing.ToDictionary(x => x.PermissionId, x => x);

        // Remove permissions that are no longer selected
        foreach (var item in existing)
        {
            if (!selectedIds.Contains(item.PermissionId))
            {
                _unitOfWork.UserPermissions.Remove(item);
            }
        }

        // Add newly selected permissions
        foreach (var permId in selectedIds)
        {
            if (!existingMap.ContainsKey(permId))
            {
                var newUserPerm = UserPermission.Create(command.UserId, permId, isGranted: true);
                _unitOfWork.UserPermissions.Add(newUserPerm);
            }
            else
            {
                var existingItem = existingMap[permId];
                if (!existingItem.IsGranted)
                {
                    existingItem.Grant();
                    _unitOfWork.UserPermissions.Update(existingItem);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_redisService != null)
        {
            try
            {
                await _redisService.RemoveAsync($"UserPermissions_{command.UserId}");
                await _redisService.RemoveAsync($"User_Info_{command.UserId}");
            }
            catch
            {
                // Invalidation failure should not fail permission saving
            }
        }

        return Result.Success();
    }
}
