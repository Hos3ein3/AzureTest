using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class UserPermissionRepository : GenericRepository<UserPermission>, IUserPermissionRepository
{
    public UserPermissionRepository(AzureTestDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<UserPermission>> GetPermissionsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(up => up.Permission)
                           .Where(up => up.UserId == userId)
                           .ToListAsync(cancellationToken);
    }

    public async Task<UserPermission?> GetUserPermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(up => up.Permission)
                           .FirstOrDefaultAsync(up => up.UserId == userId && up.PermissionId == permissionId, cancellationToken);
    }
}
