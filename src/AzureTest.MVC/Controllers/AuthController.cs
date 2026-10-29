using System;
using System.ComponentModel;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Auth;
using AzureTest.Application.Auth.Commands.ChangePassword;
using AzureTest.Application.Auth.Commands.ForgotPassword;
using AzureTest.Application.Auth.Commands.GoogleCallback;
using AzureTest.Application.Auth.Commands.Login;
using AzureTest.Application.Auth.Commands.Logout;
using AzureTest.Application.Auth.Commands.Register;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Users.Commands.RefreshToken;
using AzureTest.Contracts.DTOs.Users;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure.Configuration;
using AzureTest.MVC.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AzureTest.MVC.Controllers;

[AllowAnonymous]
[DisplayName("Auth")]
[Description("Manage and control Auth operations.")]
public class AuthController : Controller
{
    private readonly IAuthCommandUnit _commands;
    private readonly IAuthQueryUnit _queries;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly InfrastructureFeatureOptions _featureOptions;
    private readonly ICurrentUser _currentUser;

    public AuthController(
        IAuthCommandUnit commands,
        IAuthQueryUnit queries,
        SignInManager<ApplicationUser> signInManager,
        IOptions<InfrastructureFeatureOptions> featureOptions,
        ICurrentUser currentUser)
    {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _featureOptions = featureOptions?.Value ?? new InfrastructureFeatureOptions();
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    [HttpGet]
    [DisplayName("Login")]
    [Description("Execute Login view for Auth.")]
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
        var command = new LoginCommand(email, password, rememberMe);
        var result = await _commands.Login.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login failed.");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var loginResult = result.Value;
        if (!loginResult.Succeeded)
        {
            if (loginResult.IsLockedOut)
            {
                return RedirectToAction(nameof(LockedOut), new { lockoutEnd = loginResult.LockoutEnd?.ToString("o") });
            }

            if (loginResult.Errors != null)
            {
                foreach (var err in loginResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        if (_featureOptions.UseJWT)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = rememberMe ? DateTime.UtcNow.AddDays(14) : null
            };

            if (!string.IsNullOrEmpty(loginResult.Token))
                Response.Cookies.Append("jwt", loginResult.Token, cookieOptions);

            if (!string.IsNullOrEmpty(loginResult.RefreshToken))
                Response.Cookies.Append("refreshToken", loginResult.RefreshToken, cookieOptions);
        }
        else if (loginResult.User != null)
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
            };
            await _signInManager.SignInAsync(loginResult.User, authProperties);
        }

        return RedirectAfterLogin(returnUrl ?? loginResult.RedirectUrl);
    }

    [HttpGet]
    [DisplayName("Register")]
    [Description("Execute Register view for Auth.")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Register")]
    [Description("Execute Register operation for Auth.")]
    public async Task<IActionResult> Register(
        string name,
        string email,
        string password,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterCommand(name, email, password, confirmPassword);
        var result = await _commands.Register.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed.");
            return View();
        }

        var regResult = result.Value;
        if (!regResult.Succeeded)
        {
            if (regResult.Errors != null)
            {
                foreach (var err in regResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err);
                }
            }
            return View();
        }

        if (regResult.StayLoginAfterRegister && regResult.User != null)
        {
            await _signInManager.SignInAsync(regResult.User, isPersistent: true);
        }

        return LocalRedirect(regResult.RedirectUrl);
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
    public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        _ = Guid.TryParse(userIdStr, out var userId);
        await _commands.Logout.HandleAsync(new LogoutCommand(userId), cancellationToken);

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
    public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken = default)
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
        GoogleAuthMode mode,
        string? returnUrl = null,
        string? remoteError = null,
        CancellationToken cancellationToken = default)
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

        var command = new ProcessGoogleCallbackCommand(
            info.LoginProvider,
            info.ProviderKey,
            info.Principal.FindFirstValue(ClaimTypes.Email),
            info.Principal.FindFirstValue(ClaimTypes.GivenName),
            info.Principal.FindFirstValue(ClaimTypes.Surname),
            mode,
            returnUrl
        );

        var result = await _commands.GoogleCallback.HandleAsync(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return Fail(result.Error ?? "Authentication failed.");
        }

        var googleResult = result.Value;
        if (!googleResult.Succeeded)
        {
            if (googleResult.IsLockedOut)
                return RedirectToAction(nameof(LockedOut), new { lockoutEnd = googleResult.LockoutEnd?.ToString("o") });

            return Fail(googleResult.ErrorMessage ?? "Google sign-in failed.", googleResult.FallbackAction);
        }

        if (googleResult.User != null)
        {
            await _signInManager.SignInAsync(googleResult.User, isPersistent: true, info.LoginProvider);
        }

        return RedirectAfterLogin(googleResult.RedirectUrl ?? returnUrl);
    }

    [HttpGet]
    [DisplayName("Account Locked")]
    [Description("View displayed when a user account is locked.")]
    public IActionResult LockedOut(string? lockoutEnd = null, string? message = null)
    {
        ViewBag.LockoutEnd = lockoutEnd;
        ViewBag.Message = message;
        return View();
    }

    [HttpGet]
    [DisplayName("Forgot Password")]
    [Description("Display forgot password request form.")]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Forgot Password")]
    [Description("Submit forgot password request to generate reset instructions.")]
    public async Task<IActionResult> ForgotPassword(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError("email", "Email address is required.");
            return View();
        }

        var result = await _commands.ForgotPassword.HandleAsync(new ForgotPasswordCommand(email), cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to process request.");
            return View();
        }

        ViewBag.ResetToken = result.Value.ResetToken;
        ViewBag.Message = result.Value.Message;
        ViewBag.IsSubmitted = true;
        return View();
    }

    [HttpGet]
    [DisplayName("Change Password")]
    [Description("Display change password form for current user.")]
    public IActionResult ChangePassword()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(nameof(Login), new { returnUrl = Url.Action(nameof(ChangePassword)) });
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DisplayName("Change Password")]
    [Description("Submit change password request for current user.")]
    public async Task<IActionResult> ChangePassword(
        string currentPassword,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return RedirectToAction(nameof(Login), new { returnUrl = Url.Action(nameof(ChangePassword)) });
        }

        var currentUserId = _currentUser.Id;
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand(currentUserId, currentPassword, newPassword, confirmPassword);
        var result = await _commands.ChangePassword.HandleAsync(command, cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Password change failed.");
            return View();
        }

        if (!result.Value.Success)
        {
            if (result.Value.Errors != null)
            {
                foreach (var err in result.Value.Errors)
                {
                    ModelState.AddModelError(string.Empty, err);
                }
            }
            return View();
        }

        TempData["Success"] = "Your password has been changed successfully.";
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectAfterLogin(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            && !returnUrl.StartsWith("/Auth", StringComparison.OrdinalIgnoreCase)
            && !returnUrl.StartsWith("/Error", StringComparison.OrdinalIgnoreCase))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect(returnUrl ?? "/Home/Index");
    }
}
