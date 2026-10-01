

using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Services;

public interface IRoleManagerService
{
    Task<(bool Success, string[] Errors)> CreateRoleAsync(string roleName);
}
