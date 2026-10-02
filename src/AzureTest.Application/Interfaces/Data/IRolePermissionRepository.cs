using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IRolePermissionRepository : IGenericRepository<RolePermission>
{
    Task<IReadOnlyList<RolePermission>> GetPermissionsByRoleAsync(string roleName);
}
