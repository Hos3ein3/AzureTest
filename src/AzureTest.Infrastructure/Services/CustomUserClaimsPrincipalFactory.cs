using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AzureTest.Infrastructure.Services;

public class CustomUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    public CustomUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {
    }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        
        if (!string.IsNullOrEmpty(user.FirstName))
            identity.AddClaim(new Claim("FirstName", user.FirstName));
            
        if (!string.IsNullOrEmpty(user.LastName))
            identity.AddClaim(new Claim("LastName", user.LastName));
            
        if (!string.IsNullOrEmpty(user.ProfileImage))
            identity.AddClaim(new Claim("ProfileImage", user.ProfileImage));

        return identity;
    }
}
