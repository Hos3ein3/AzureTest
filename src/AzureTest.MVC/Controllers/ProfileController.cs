using System;
using System.IO;
using System.Threading.Tasks;
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
    public async Task<IActionResult> Update(string firstName, string lastName, string phoneNumber, IFormFile? profileImage)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FirstName = firstName ?? "";
        user.LastName = lastName ?? "";
        user.PhoneNumber = phoneNumber;

        if (profileImage != null && profileImage.Length > 0)
        {
            var uploads = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploads);
            
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
            var filePath = Path.Combine(uploads, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await profileImage.CopyToAsync(stream);
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
