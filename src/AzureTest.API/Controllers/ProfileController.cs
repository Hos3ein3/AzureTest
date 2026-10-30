using System;
using System.IO;
using System.Threading.Tasks;
using Asp.Versioning;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Profile;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly IWebHostEnvironment _env;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        ICurrentUser currentUser,
        IWebHostEnvironment env)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ProfileImage = user.ProfileImage
        };

        return Ok(new { success = true, profile = profileDto });
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] UpdateProfileRequestDto request)
    {
        return await ProcessUpdateAsync(request);
    }

    [HttpPut]
    [Consumes("application/json")]
    public async Task<IActionResult> UpdateJson([FromBody] UpdateProfileRequestDto request)
    {
        return await ProcessUpdateAsync(request);
    }

    private async Task<IActionResult> ProcessUpdateAsync(UpdateProfileRequestDto request)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
            return NotFound(new { success = false, message = "User not found." });

        if (request.FirstName != null)
            user.FirstName = request.FirstName;
        if (request.LastName != null)
            user.LastName = request.LastName;
        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        if (request.ProfileImage != null && request.ProfileImage.Length > 0)
        {
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploads = Path.Combine(webRoot, "uploads", "profiles");
            Directory.CreateDirectory(uploads);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ProfileImage.FileName);
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.ProfileImage.CopyToAsync(stream);
            }

            user.ProfileImage = "/uploads/profiles/" + fileName;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                success = false,
                message = "Failed to update profile.",
                errors = result.Errors
            });
        }

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            ProfileImage = user.ProfileImage
        };

        return Ok(new
        {
            success = true,
            message = "Profile updated successfully!",
            profile = profileDto
        });
    }

    private async Task<ApplicationUser?> GetCurrentUserAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null && _currentUser.Id != Guid.Empty)
        {
            user = await _userManager.FindByIdAsync(_currentUser.Id.ToString());
        }
        return user;
    }
}
