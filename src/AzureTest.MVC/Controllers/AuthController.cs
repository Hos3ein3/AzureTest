using AzureTest.Application.Messaging;
using AzureTest.Application.Users;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure.Configuration;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using AzureTest.Application.Users.Commands.LoginUser;
using AzureTest.Application.Users.Commands.RegisterUser;
using AzureTest.Application.Users.Commands.LogoutUser;
using AzureTest.Application.Users.Commands.RefreshToken;

namespace AzureTest.MVC.Controllers;

[AllowAnonymous]
public class AuthController : Controller
{
    private readonly IUserCommandUnit _commands;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly InfrastructureFeatureOptions _featureOptions;

    public AuthController(
        IUserCommandUnit commands,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IOptions<InfrastructureFeatureOptions> featureOptions)
    {
        _commands = commands;
        _signInManager = signInManager;
        _userManager = userManager;
        _featureOptions = featureOptions.Value;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, CancellationToken cancellationToken)
    {
        var result = await _commands.Login.HandleAsync(new LoginUserCommand(email, password), cancellationToken);
        if (result.IsSuccess && result.Value.Success)
        {
            if (_featureOptions.UseJWT)
            {
                // Store JWT in cookie
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
            }
            else
            {
                // Use Identity Cookie SignIn
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: true);
                }
            }
            
            return RedirectToAction("Index", "Home");
        }
        if (result.IsSuccess && result.Value.Errors != null)
        {
            foreach (var error in result.Value.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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
            return RedirectToAction(nameof(Login));
        }

        if (result.Error != null)
        {
            ModelState.AddModelError(string.Empty, result.Error);
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
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
