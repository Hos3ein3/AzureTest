using System;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.Profile;
using AzureTest.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AzureTest.API.Feature.Profile;

public static class GetProfile
{
    public static async Task<IResult> HandleAsync(
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] ICurrentUser currentUser)
    {
        var userId = currentUser.Id;
        if (userId == Guid.Empty)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Results.NotFound(new { success = false, message = "User not found." });

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

        return Results.Ok(new { success = true, profile = profileDto });
    }
}
