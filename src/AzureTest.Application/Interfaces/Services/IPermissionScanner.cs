using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Permissions;

namespace AzureTest.Application.Interfaces.Services;

public interface IPermissionScanner
{
    List<DiscoveredPermissionDto> ScanPermissions();
    HashSet<string> GetAdminOnlyEndpoints();
}
