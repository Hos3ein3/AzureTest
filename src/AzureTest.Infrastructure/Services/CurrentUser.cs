

using System;
using System.Linq;
using System.Security.Claims;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace AzureTest.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    public const string ProfileImageClaim = "ProfileImage";
    public const string FirstNameClaim = "FirstName";
    public const string LastNameClaim = "LastName";
    public const string FullNameClaim = "FullName";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid Id
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User?.FindFirst("sub")?.Value;

            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }
    }

    public string Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value
        ?? string.Empty;

    public string? PhoneNumber =>
        User?.FindFirst(ClaimTypes.MobilePhone)?.Value
        ?? User?.FindFirst("PhoneNumber")?.Value
        ?? User?.FindFirst("phone_number")?.Value;

    public string? ProfileImage =>
        User?.FindFirst(ProfileImageClaim)?.Value
        ?? User?.FindFirst("profile_image")?.Value;

    public string FirstName =>
        User?.FindFirst(FirstNameClaim)?.Value
        ?? User?.FindFirst(ClaimTypes.GivenName)?.Value
        ?? string.Empty;

    public string LastName =>
        User?.FindFirst(LastNameClaim)?.Value
        ?? User?.FindFirst(ClaimTypes.Surname)?.Value
        ?? string.Empty;

    public string FullName
    {
        get
        {
            var fullNameClaim = User?.FindFirst(FullNameClaim)?.Value;
            if (!string.IsNullOrWhiteSpace(fullNameClaim))
            {
                return fullNameClaim;
            }

            var first = FirstName;
            var last = LastName;
            var combined = $"{first} {last}".Trim();
            return string.IsNullOrWhiteSpace(combined) ? string.Empty : combined;
        }
    }

    public System.Collections.Generic.IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() 
        ?? new System.Collections.Generic.List<string>();
}
