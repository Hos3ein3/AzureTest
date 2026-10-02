using System.Collections.Generic;
using System.Threading.Tasks;
using AzureTest.Domain.Entities;

namespace AzureTest.Application.Interfaces.Data;

public interface IPermissionRepository : IGenericRepository<Permission>
{
    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync();
}
