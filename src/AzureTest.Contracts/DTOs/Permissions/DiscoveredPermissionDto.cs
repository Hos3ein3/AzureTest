namespace AzureTest.Contracts.DTOs.Permissions;

public record DiscoveredPermissionDto(
    string ControllerName,
    string ActionName,
    string DisplayName,
    string Description
);
