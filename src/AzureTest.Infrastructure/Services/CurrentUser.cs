using System;
using System.Linq;
using System.Security.Claims;
using AzureTest.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AzureTest.Infrastructure.Services;

public class CurrentUser : ICurrentUser
{
    public const string ProfileImageClaim = "ProfileImage";
    public const string FirstNameClaim = "FirstName";
    public const string LastNameClaim = "LastName";
    public const string FullNameClaim = "FullName";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;

    public CurrentUser(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
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
    
    // Helper to get cached user session from Redis
    private AzureTest.Contracts.DTOs.Users.UserSessionCacheDto? GetCachedSession()
    {
        var id = Id;
        if (id == Guid.Empty) return null;
        
        var redis = _serviceProvider.GetService<IRedisService>();
        if (redis == null) return null;
        
        return redis.GetAsync<AzureTest.Contracts.DTOs.Users.UserSessionCacheDto>($"User_Info_{id}").GetAwaiter().GetResult();
    }

    public string Email =>
        GetCachedSession()?.Email 
        ?? User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value
        ?? string.Empty;

    public string? PhoneNumber =>
        GetCachedSession()?.PhoneNumber
        ?? User?.FindFirst(ClaimTypes.MobilePhone)?.Value
        ?? User?.FindFirst("PhoneNumber")?.Value
        ?? User?.FindFirst("phone_number")?.Value;

    public string? ProfileImage =>
        GetCachedSession()?.ProfileImage
        ?? User?.FindFirst(ProfileImageClaim)?.Value
        ?? User?.FindFirst("profile_image")?.Value;

    public string FirstName =>
        GetCachedSession()?.FirstName
        ?? User?.FindFirst(FirstNameClaim)?.Value
        ?? User?.FindFirst(ClaimTypes.GivenName)?.Value
        ?? string.Empty;

    public string LastName =>
        GetCachedSession()?.LastName
        ?? User?.FindFirst(LastNameClaim)?.Value
        ?? User?.FindFirst(ClaimTypes.Surname)?.Value
        ?? string.Empty;

    public string FullName
    {
        get
        {
            var cached = GetCachedSession();
            if (cached != null && !string.IsNullOrWhiteSpace(cached.FullName)) return cached.FullName;

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
        GetCachedSession()?.Roles
        ?? User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() 
        ?? new System.Collections.Generic.List<string>();
        
    public System.Collections.Generic.IReadOnlyList<string> Permissions =>
        GetCachedSession()?.Permissions
        ?? new System.Collections.Generic.List<string>();
}
