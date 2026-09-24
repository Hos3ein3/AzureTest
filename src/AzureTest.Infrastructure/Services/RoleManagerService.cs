using System.Linq;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Infrastructure.Services;

public class RoleManagerService : IRoleManagerService
{
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RoleManagerService(RoleManager<ApplicationRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task<(bool Success, string[] Errors)> CreateRoleAsync(string roleName)
    {
        var role = new ApplicationRole { Name = roleName };
        var result = await _roleManager.CreateAsync(role);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }
}
