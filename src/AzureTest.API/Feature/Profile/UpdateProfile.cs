using System;
using System.IO;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Profile;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Profile;

public static class UpdateProfile
{
    public static async Task<IResult> HandleAsync(
        [FromForm] UpdateProfileRequestDto request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] ICurrentUser currentUser,
        [FromServices] IWebHostEnvironment env)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Results.NotFound(new { success = false, message = "User not found." });

        if (request.FirstName != null)
            user.FirstName = request.FirstName;
        if (request.LastName != null)
            user.LastName = request.LastName;
        if (request.PhoneNumber != null)
            user.PhoneNumber = request.PhoneNumber;

        if (request.ProfileImage != null && request.ProfileImage.Length > 0)
        {
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
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

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Results.BadRequest(new
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

        return Results.Ok(new
        {
            success = true,
            message = "Profile updated successfully!",
            profile = profileDto
        });
    }
}
