using AzureTest.Application.Messaging;
using AzureTest.Application.Users;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure.Configuration;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel;
using System.Security.Claims;

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
using AzureTest.MVC.Filters;

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
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
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
        string? returnUrl = null,
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
                        var areaPrefix = !string.IsNullOrWhiteSpace(rp.Permission.AreaName)
                            ? $"{rp.Permission.AreaName}/"
                            : "";
                        permissions.Add($"{areaPrefix}{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
                        // Also keep short key for compatibility
                        permissions.Add($"{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
                    }
                }

                var sessionDto = new UserSessionCacheDto
                {
                    Id = userObj.Id,
                    Email = userObj.Email,
                    PhoneNumber = userObj.PhoneNumber,
                    ProfileImage = userObj.ProfileImage,
                    FirstName = userObj.FirstName,
                    LastName = userObj.LastName,
                    FullName = userObj.FullName,
                    Roles = System.Linq.Enumerable.ToList(roles),
                    Permissions = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Distinct(permissions))
                };
                await _redisService.SetAsync($"User_Info_{userObj.Id}", sessionDto, TimeSpan.FromDays(7));
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) &&
                !returnUrl.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase) &&
                !returnUrl.StartsWith("/Error", StringComparison.OrdinalIgnoreCase))
            {
                return LocalRedirect(returnUrl);
            }

            var settingRes = await _getIdentitySettingQuery.HandleAsync(new GetIdentitySettingQuery());
            var redirectUrl = settingRes.IsSuccess ? settingRes.Value.RedirectAfterLogin : "/Home/Index";
            return LocalRedirect(redirectUrl ?? "/Home/Index");
        }

        if (result.IsSuccess && result.Value.Errors != null)
        {
            foreach (var error in result.Value.Errors) ModelState.AddModelError(string.Empty, error);
        }

        ViewData["ReturnUrl"] = returnUrl;
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
    public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword,
        CancellationToken cancellationToken)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return View();
        }

        var nameParts = (name ?? string.Empty).Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
        var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var result = await _commands.Register.HandleAsync(new RegisterUserCommand(email, password, firstName, lastName),
            cancellationToken);
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
    [AllowImpersonationWrite]
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
            var cookieOptions = new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict };
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

        var result =
            await _commands.RefreshToken.HandleAsync(new RefreshTokenCommand(jwt, refreshToken), cancellationToken);

        if (result.IsSuccess && result.Value.Success)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddDays(1)
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

    [HttpPost, ValidateAntiForgeryToken]
    [DisplayName("Google Sign In")]
    [Description("Sign in with Google. Account must already exist.")]
    public IActionResult SignInByGoogle(string? returnUrl = null)
        => StartGoogle(GoogleAuthMode.SignInOnly, returnUrl);

    [HttpPost, ValidateAntiForgeryToken]
    [DisplayName("Google Register")]
    [Description("Register with Google. Account must not exist.")]
    public IActionResult RegisterByGoogle(string? returnUrl = null)
        => StartGoogle(GoogleAuthMode.RegisterOnly, returnUrl);

    [HttpPost, ValidateAntiForgeryToken]
    [DisplayName("Google Register And Sign In")]
    [Description("Sign in with Google, or register if the account is new.")]
    public IActionResult RegisterAndSignInByGoogle(string? returnUrl = null)
        => StartGoogle(GoogleAuthMode.SignInOrRegister, returnUrl);

    private IActionResult StartGoogle(GoogleAuthMode mode, string? returnUrl)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "Auth", new { mode, returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);
        return Challenge(properties, "Google");
    }

    [HttpGet]
[DisplayName("Google Callback")]
[Description("Handle the return from Google.")]
public async Task<IActionResult> GoogleCallback(
    GoogleAuthMode mode, string? returnUrl = null, string? remoteError = null)
{
    var defaultAction = mode == GoogleAuthMode.RegisterOnly ? nameof(Register) : nameof(Login);

    IActionResult Fail(string message, string? action = null)
    {
        TempData["Error"] = message;
        return RedirectToAction(action ?? defaultAction, new { returnUrl });
    }

    if (!string.IsNullOrEmpty(remoteError))
        return Fail($"Google error: {remoteError}");

    var info = await _signInManager.GetExternalLoginInfoAsync();
    if (info == null)
        return Fail("Could not read Google login information.");

    // Case 1: this Google account is already linked
    var linkedUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
    if (linkedUser != null)
    {
        if (mode == GoogleAuthMode.RegisterOnly)
            return Fail("This Google account is already registered. Please sign in.", nameof(Login));

        var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);

        if (result.IsLockedOut) return RedirectToAction(nameof(AccessDenied));
        if (!result.Succeeded) return Fail("Sign in with Google failed.", nameof(Login));

        await CacheUserSessionAsync(linkedUser);
        return await RedirectAfterLoginAsync(returnUrl);
    }

    // Case 2: not linked yet, so look at the email
    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
    if (string.IsNullOrWhiteSpace(email))
        return Fail("Google did not return an email address.");

    var user = await _userManager.FindByEmailAsync(email);

    if (user == null && mode == GoogleAuthMode.SignInOnly)
        return Fail("No account exists for this email. Please register first.", nameof(Register));

    if (user != null && mode == GoogleAuthMode.RegisterOnly)
        return Fail("This email is already registered. Please sign in.", nameof(Login));

    if (user == null)
    {
        user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
            LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty
        };

        var create = await _userManager.CreateAsync(user);
        if (!create.Succeeded)
            return Fail(string.Join(" ", create.Errors.Select(e => e.Description)));

        var settingRes = await _getIdentitySettingQuery.HandleAsync(new GetIdentitySettingQuery());
        var setting = settingRes.IsSuccess ? settingRes.Value : new IdentitySetting();
        foreach (var role in setting.RolesForRegister ?? Enumerable.Empty<string>())
            if (!await _userManager.IsInRoleAsync(user, role))
                await _userManager.AddToRoleAsync(user, role);
    }
    else if (!user.EmailConfirmed)
    {
        // Safety: someone could have pre-registered this email with a password they know.
        // Google proves that the real owner is here now, so remove the old password.
        await _userManager.RemovePasswordAsync(user);
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
    }

    var addLogin = await _userManager.AddLoginAsync(user, info);
    if (!addLogin.Succeeded)
        return Fail(string.Join(" ", addLogin.Errors.Select(e => e.Description)));

    await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);
    await CacheUserSessionAsync(user);
    return await RedirectAfterLoginAsync(returnUrl);
}
    public enum GoogleAuthMode
    {
        SignInOnly,
        RegisterOnly,
        SignInOrRegister
    }

    private async Task CacheUserSessionAsync(ApplicationUser userObj)
    {
        var roles = await _userManager.GetRolesAsync(userObj);
        var permissions = new List<string>();
        foreach (var role in roles)
        {
            var rolePerms = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(role);
            foreach (var rp in rolePerms)
            {
                var areaPrefix = !string.IsNullOrWhiteSpace(rp.Permission.AreaName) ? $"{rp.Permission.AreaName}/" : "";
                permissions.Add($"{areaPrefix}{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
                permissions.Add($"{rp.Permission.ControllerName}/{rp.Permission.ActionName}");
            }
        }

        var sessionDto = new UserSessionCacheDto
        {
            Id = userObj.Id,
            Email = userObj.Email,
            PhoneNumber = userObj.PhoneNumber,
            ProfileImage = userObj.ProfileImage,
            FirstName = userObj.FirstName,
            LastName = userObj.LastName,
            FullName = userObj.FullName,
            Roles = roles.ToList(),
            Permissions = permissions.Distinct().ToList()
        };
        await _redisService.SetAsync($"User_Info_{userObj.Id}", sessionDto, TimeSpan.FromDays(7));
    }

    private async Task<IActionResult> RedirectAfterLoginAsync(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                                                  && !returnUrl.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase)
                                                  && !returnUrl.StartsWith("/Error",
                                                      StringComparison.OrdinalIgnoreCase))
            return LocalRedirect(returnUrl);

        var settingRes = await _getIdentitySettingQuery.HandleAsync(new GetIdentitySettingQuery());
        var redirectUrl = settingRes.IsSuccess ? settingRes.Value.RedirectAfterLogin : "/Home/Index";
        return LocalRedirect(redirectUrl ?? "/Home/Index");
    }
}