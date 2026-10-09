import os

path = "src/AzureTest.MVC/Controllers/AuthController.cs"
with open(path, "r") as f:
    content = f.read()

# Add GetIdentitySettingQueryHandler dependency
if "GetIdentitySettingQueryHandler" not in content:
    content = content.replace("using Microsoft.Extensions.Options;", "using Microsoft.Extensions.Options;\nusing AzureTest.Application.IdentitySettings;\nusing AzureTest.Domain.Documents;")
    
    content = content.replace("private readonly IUnitOfWork _unitOfWork;", "private readonly IUnitOfWork _unitOfWork;\n    private readonly GetIdentitySettingQueryHandler _getIdentitySettingQuery;")
    
    content = content.replace("IUnitOfWork unitOfWork)", "IUnitOfWork unitOfWork,\n        GetIdentitySettingQueryHandler getIdentitySettingQuery)")
    
    content = content.replace("_unitOfWork = unitOfWork;", "_unitOfWork = unitOfWork;\n        _getIdentitySettingQuery = getIdentitySettingQuery;")

# Rewrite Login
login_start = content.find("public async Task<IActionResult> Login(string email, string password, CancellationToken cancellationToken)")
login_end = content.find("return View();\n    }", login_start) + len("return View();\n    }")

new_login = """public async Task<IActionResult> Login(string email, string password, CancellationToken cancellationToken)
    {
        var result = await _commands.Login.HandleAsync(new LoginUserCommand(email, password), cancellationToken);
        if (result.IsSuccess && result.Value.Success)
        {
            if (_featureOptions.UseJWT)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTime.UtcNow.AddDays(1)
                };
                Response.Cookies.Append("jwt", result.Value.Token!, cookieOptions);
                if (!string.IsNullOrEmpty(result.Value.RefreshToken))
                    Response.Cookies.Append("refreshToken", result.Value.RefreshToken, cookieOptions);
            }
            else
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null) await _signInManager.SignInAsync(user, isPersistent: true);
            }
            
            var userObj = await _userManager.FindByEmailAsync(email);
            if (userObj != null)
            {
                var roles = await _userManager.GetRolesAsync(userObj);
                var permissions = new System.Collections.Generic.List<string>();
                foreach (var role in roles)
                {
                    var rolePerms = await _unitOfWork.RolePermissions.GetPermissionsByRoleAsync(role);
                    permissions.AddRange(System.Linq.Enumerable.Select(rolePerms, rp => rp.Permission.ControllerName + "/" + rp.Permission.ActionName));
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
    }"""

content = content[:login_start] + new_login + content[login_end:]

# Rewrite Register
reg_start = content.find("public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword, CancellationToken cancellationToken)")
reg_end = content.find("return View();\n    }", reg_start) + len("return View();\n    }")

new_register = """public async Task<IActionResult> Register(string name, string email, string password, string confirmPassword, CancellationToken cancellationToken)
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
    }"""

content = content[:reg_start] + new_register + content[reg_end:]

with open(path, "w") as f:
    f.write(content)
print("AuthController patched")
