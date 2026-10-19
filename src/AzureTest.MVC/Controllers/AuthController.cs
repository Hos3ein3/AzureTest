using AzureTest.Application.Messaging;
using AzureTest.Application.Users;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure.Configuration;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using Microsoft.Extensions.Options;
using AzureTest.Application.IdentitySettings;
using AzureTest.Domain.Documents;

using AzureTest.Application.Users.Commands.LoginUser;
using AzureTest.Application.Users.Commands.RegisterUser;
using AzureTest.Application.Users.Commands.LogoutUser;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Users;

namespace AzureTest.MVC.Controllers;

[AllowAnonymous]
[DisplayName("Auth")]
[Description("Manage and control Auth operations.")]
    public class AuthController : Controller
{
    private readonly IUserCommandUnit _commands;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InfrastructureFeatureOptions _featureOptions;
    private readonly IRedisService _redisService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly GetIdentitySettingQueryHandler _getIdentitySettingQuery;

    public AuthController(
        IUserCommandUnit commands,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IOptions<InfrastructureFeatureOptions> featureOptions,
        IRedisService redisService,
        IUnitOfWork unitOfWork,
        GetIdentitySettingQueryHandler getIdentitySettingQuery)
    {
        _commands = commands;
        _signInManager = signInManager;
        _userManager = userManager;
        _featureOptions = featureOptions.Value;
        _redisService = redisService;
        _unitOfWork = unitOfWork;
        _getIdentitySettingQuery = getIdentitySettingQuery;
    }

    [HttpGet]
    [DisplayName("Login")]
    [Description("Execute Login operation for Auth.")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Login")]
    [Description("Execute Login operation for Auth.")]
    public async Task<IActionResult> Login(
        string email, 
        string password, 
        [FromForm(Name = "remember-me")] bool rememberMe = true, 
        CancellationToken cancellationToken = default)
    {
        var result = await _commands.Login.HandleAsync(new LoginUserCommand(email, password), cancellationToken);
        if (result.IsSuccess && result.Value.Success)
        {
            if (_featureOptions.UseJWT)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, 
                    Secure = Request.IsHttps, 
                    SameSite = SameSiteMode.Lax, 
                    Expires = rememberMe ? DateTime.UtcNow.AddDays(14) : null
                };
                Response.Cookies.Append("jwt", result.Value.Token!, cookieOptions);
                if (!string.IsNullOrEmpty(result.Value.RefreshToken))
                    Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var authProperties = new Microsoft.AspNetCore.Authentication.AuthenticationProperties
                    {
                        IsPersistent = rememberMe,
                        ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
                    };
                    await _signInManager.SignInAsync(user, authProperties);
                }
            }
            
            var userObj = await _userManager.FindByEmailAsync(email);
            if (userObj != null)
            {
                var roles = await _userManager.GetRolesAsync(userObj);
                var permissions = new System.Collections.Generic.List<string>();
                foreach (var role in roles)
                {
                    var rolePerms = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(role);
                    foreach (var rp in rolePerms)
                    {
                        var areaPrefix = !string.IsNullOrWhiteSpace(rp.Permission.AreaName) ? $"{rp.Permission.AreaName}/" : "";
                        permissions.Add($"{areaPrefix}{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
                        // Also keep short key for compatibility
                        permissions.Add($"{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
                    }
                }
                
                var sessionDto = new UserSessionCacheDto
                {
                    Id = userObj.Id, Email = userObj.Email, PhoneNumber = userObj.PhoneNumber,
                    ProfileImage = userObj.ProfileImage, FirstName = userObj.FirstName,
                    LastName = userObj.LastName, FullName = userObj.FullName,
                    Roles = System.Linq.Enumerable.ToList(roles),
                    Permissions = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(permissions))
                };
                await _redisService.SetAsync($"User_Info_{userObj.Id}", sessionDto, TimeSpan.FromDays(7));
            }

            var settingRes = await _getIdentitySettingQuery.HandleAsync(new GetIdentitySettingQuery());
            var redirectUrl = settingRes.IsSuccess ? settingRes.Value.RedirectAfterLogin : "/Home/Index";
            return LocalRedirect(redirectUrl ?? "/Home/Index");
        }
        if (result.IsSuccess && result.Value.Errors != null)
        {
            foreach (var error in result.Value.Errors) ModelState.AddModelError(string.Empty, error);
        }
        return View();
    }

    [HttpGet]
    [DisplayName("Register")]
    [Description("Execute Register operation for Auth.")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Register")]
    [Description("Execute Register operation for Auth.")]
    public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword, CancellationToken cancellationToken)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return View();
        }

        var nameParts = (name ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var result = await _commands.Register.HandleAsync(new RegisterUserCommand(email, password, firstName, lastName), cancellationToken);
        if (result.IsSuccess)
        {
            var settingRes = await _getIdentitySettingQuery.HandleAsync(new GetIdentitySettingQuery());
            var setting = settingRes.IsSuccess ? settingRes.Value : new IdentitySetting();

            var userObj = await _userManager.FindByEmailAsync(email);
            if (userObj != null)
            {
                if (setting.RolesForRegister != null && setting.RolesForRegister.Any())
                {
                    foreach (var role in setting.RolesForRegister)
                    {
                        if (!await _userManager.IsInRoleAsync(userObj, role))
                        {
                            await _userManager.AddToRoleAsync(userObj, role);
                        }
                    }
                }

                if (setting.StayLoginAfterRegister)
                {
                    await _signInManager.SignInAsync(userObj, isPersistent: true);
                    return LocalRedirect(setting.RedirectAfterLogin ?? "/");
                }
            }

            return LocalRedirect(setting.RedirectAfterRegister ?? "/Auth/Login");
        }

        if (result.Error != null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
        }
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    [DisplayName("Access Denied")]
    [Description("Show access denied page.")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpPost]
    [DisplayName("Logout")]
    [Description("Execute Logout operation for Auth.")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                     ?? User.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await _redisService.RemoveAsync($"User_Info_{userId}");
        }
        
        await _commands.Logout.HandleAsync(new LogoutUserCommand(), cancellationToken);

        
        if (_featureOptions.UseJWT)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Delete("jwt", cookieOptions);
            Response.Cookies.Delete("refreshToken", cookieOptions);
        }
        else
        {
            await _signInManager.SignOutAsync();
        }
        
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [Route("refresh-token")]
    [DisplayName("Refresh Token")]
    [Description("Execute Refresh Token operation for Auth.")]
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
    {
        var jwt = Request.Cookies["jwt"];
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(jwt) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var result = await _commands.RefreshToken.HandleAsync(new RefreshTokenCommand(jwt, refreshToken), cancellationToken);

        if (result.IsSuccess && result.Value.Success)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(1)
            };
            
            Response.Cookies.Append("jwt", result.Value.Token!, cookieOptions);
            
            if (!string.IsNullOrEmpty(result.Value.RefreshToken))
            {
                Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);
            }
            
            return Ok();
        }

        return Unauthorized();
    }
}
