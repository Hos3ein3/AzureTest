using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace AzureTest.Infrastructure.Seeding;

public class IdentityDataSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<IdentityDataSeeder> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            var systemRoleName = "system";
            var adminRoleName = "Admin";
            var userRoleName = "User";

            if (!await _roleManager.RoleExistsAsync(systemRoleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = systemRoleName });

            if (!await _roleManager.RoleExistsAsync(adminRoleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = adminRoleName });

            if (!await _roleManager.RoleExistsAsync(userRoleName))
                await _roleManager.CreateAsync(new ApplicationRole { Name = userRoleName });

            // Create system user
            var systemUserEmail = "system@azuretest.local";
            if (await _userManager.FindByEmailAsync(systemUserEmail) == null)
            {
                var systemUser = new ApplicationUser
                {
                    UserName = systemUserEmail,
                    Email = systemUserEmail,
                    FirstName = "System",
                    LastName = "Account",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(systemUser, "System!123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(systemUser, systemRoleName);
                }
            }

            // Create admin user
            var adminUserEmail = "admin@azuretest.local";
            if (await _userManager.FindByEmailAsync(adminUserEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminUserEmail,
                    Email = adminUserEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, adminRoleName);
                    
                }
            }
            var userUserEmail = "mohammadhosein.behzadifard@gmail.com";
            if (await _userManager.FindByEmailAsync(userUserEmail) == null)
            {
                var userUser = new ApplicationUser
                {
                    UserName = userUserEmail,
                    Email = userUserEmail,
                    FirstName = "Mohammad Hosein",
                    LastName = "Behzadifard",
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(userUser, "123456");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(userUser, userRoleName);
                    
                }
            }
            
            
            
            _logger.LogInformation("Identity data seeded successfully.");
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error while seeding Identity data.");
        }
    }
}
