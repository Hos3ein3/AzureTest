using System;
using Microsoft.AspNetCore.Http;

namespace AzureTest.Contracts.DTOs.Profile;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
}

public class UpdateProfileRequestDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
