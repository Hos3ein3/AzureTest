using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IUserPermissionRepository : IGenericRepository<UserPermission>
{
    Task<IReadOnlyList<UserPermission>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPermission?> GetUserPermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default);
}
