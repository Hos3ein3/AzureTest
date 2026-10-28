using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Permissions;
using FluentValidation;

namespace AzureTest.Application.UserPermissions.Queries.GetUserPermissions;

public record GetUserPermissionsQuery(Guid UserId) : IQuery<Result<IReadOnlyList<UserPermissionDto>>>;

public class GetUserPermissionsQueryValidator : AbstractValidator<GetUserPermissionsQuery>
{
    public GetUserPermissionsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

public class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, Result<IReadOnlyList<UserPermissionDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserPermissionsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<Result<IReadOnlyList<UserPermissionDto>>> HandleAsync(
        GetUserPermissionsQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.UserId == Guid.Empty)
        {
            return Result<IReadOnlyList<UserPermissionDto>>.Failure("User ID cannot be empty.");
        }

        var userPermissions = await _unitOfWork.UserPermissions.GetPermissionsByUserIdAsync(query.UserId, cancellationToken);

        var dtos = userPermissions.Select(up => new UserPermissionDto(
            up.Id,
            up.UserId,
            up.PermissionId,
            up.IsGranted,
            up.Permission?.ControllerName ?? string.Empty,
            up.Permission?.ActionName ?? string.Empty,
            up.Permission?.DisplayName ?? string.Empty,
            up.Permission?.Description ?? string.Empty,
            up.Permission?.IsRestricted ?? false,
            up.Permission?.AreaName
        )).ToList();

        return Result<IReadOnlyList<UserPermissionDto>>.Success(dtos);
    }
}
