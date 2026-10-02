using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class PermissionRepository : GenericRepository<Permission>, IPermissionRepository
{
    public PermissionRepository(AzureTestDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync()
    {
        return await _dbSet.ToListAsync();
    }
}
