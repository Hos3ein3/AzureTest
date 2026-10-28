using System;

namespace AzureTest.Contracts.DTOs.Permissions;

public record UserPermissionDto(
    Guid Id,
    Guid UserId,
    Guid PermissionId,
    bool IsGranted,
    string ControllerName,
    string ActionName,
    string DisplayName,
    string Description,
    bool IsRestricted,
    string? AreaName = null
);
