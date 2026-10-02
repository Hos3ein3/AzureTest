using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class RolePermissionRepository : GenericRepository<RolePermission>, IRolePermissionRepository
{
    public RolePermissionRepository(AzureTestDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<RolePermission>> GetPermissionsByRoleAsync(string roleName)
    {
        return await _dbSet.Include(rp => rp.Permission)
                           .Where(rp => rp.RoleName == roleName)
                           .ToListAsync();
    }
}
