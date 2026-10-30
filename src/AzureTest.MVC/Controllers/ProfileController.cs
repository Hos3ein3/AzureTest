using System;
using System.IO;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Profile;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.MVC.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromForm] UpdateProfileRequestDto request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FirstName = request.FirstName ?? "";
        user.LastName = request.LastName ?? "";
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
        
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profile updated successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to update profile.";
        }

        return RedirectToAction("Index");
    }
}
