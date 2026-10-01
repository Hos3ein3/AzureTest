

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Infrastructure.Configuration;
using AzureTest.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AzureTest.Infrastructure.Services;

public class UserManagerService : IUserManagerService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly AzureTestDbContext _dbContext;

    public UserManagerService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        AzureTestDbContext dbContext)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _dbContext = dbContext;
    }

    public async Task<(bool Success, string[] Errors)> RegisterUserAsync(string email, string password, string firstName, string lastName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, "User");
        }
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<bool> CheckPasswordAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<AuthTokensDto> GenerateJwtTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return new AuthTokensDto(null, null);

        var jti = Guid.NewGuid().ToString();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim("FirstName", user.FirstName ?? string.Empty),
            new Claim("LastName", user.LastName ?? string.Empty),
            new Claim("ProfileImage", user.ProfileImage ?? string.Empty)
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var keyString = _jwtSettings.Key;
        if (string.IsNullOrEmpty(keyString))
            throw new InvalidOperationException("JWT Key is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var issuer = _jwtSettings.Issuer ?? "Issuer";
        var audience = _jwtSettings.Audience ?? "Audience";
        var expirationSeconds = _jwtSettings.ExpirationSeconds > 0 ? _jwtSettings.ExpirationSeconds : 3600;

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expirationSeconds),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        var expirationMinutes = _jwtSettings.RefreshTokenExpirationMinutes > 0
            ? _jwtSettings.RefreshTokenExpirationMinutes
            : 1440;

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            JwtId = jti,
            CreationDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Used = false,
            Invalidated = false,
            UserId = user.Id
        };

        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return new AuthTokensDto(tokenString, refreshTokenEntity.Token);
    }

    public async Task<AuthResultDto> RefreshTokenAsync(string token, string refreshToken)
    {
        var validatedToken = GetPrincipalFromToken(token);
        if (validatedToken == null)
        {
            return new AuthResultDto(false, new[] { "Invalid token." }, null, null);
        }

        var jti = validatedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti || c.Type == "jti")?.Value;
        var userIdString = validatedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == JwtRegisteredClaimNames.Sub || c.Type == "sub")?.Value;

        if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
        {
            return new AuthResultDto(false, new[] { "Invalid token claims." }, null, null);
        }

        var storedRefreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (storedRefreshToken == null)
        {
            return new AuthResultDto(false, new[] { "This refresh token does not exist." }, null, null);
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
        {
            return new AuthResultDto(false, new[] { "This refresh token has expired." }, null, null);
        }

        if (storedRefreshToken.Invalidated)
        {
            return new AuthResultDto(false, new[] { "This refresh token has been invalidated." }, null, null);
        }

        if (storedRefreshToken.Used)
        {
            return new AuthResultDto(false, new[] { "This refresh token has already been used." }, null, null);
        }

        if (storedRefreshToken.JwtId != jti)
        {
            return new AuthResultDto(false, new[] { "This refresh token does not match this JWT." }, null, null);
        }

        if (storedRefreshToken.UserId != userId)
        {
            return new AuthResultDto(false, new[] { "This refresh token does not belong to this user." }, null, null);
        }

        storedRefreshToken.Used = true;
        _dbContext.RefreshTokens.Update(storedRefreshToken);
        await _dbContext.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            return new AuthResultDto(false, new[] { "User does not exist." }, null, null);
        }

        var newTokenResult = await GenerateJwtTokenAsync(user.Email);
        if (newTokenResult.Token == null || newTokenResult.RefreshToken == null)
        {
            return new AuthResultDto(false, new[] { "Failed to generate new token." }, null, null);
        }

        return new AuthResultDto(true, Array.Empty<string>(), newTokenResult.Token, newTokenResult.RefreshToken);
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var keyString = _jwtSettings.Key;
            if (string.IsNullOrEmpty(keyString))
                return null;

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
    {
        return validatedToken is JwtSecurityToken jwtSecurityToken &&
               (jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase) ||
                jwtSecurityToken.Header.Alg.Equals("HS256", StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task<(bool Success, string[] Errors)> AddUserToRoleAsync(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return (false, new[] { "User not found." });

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return (result.Succeeded, result.Errors.Select(e => e.Description).ToArray());
    }
}
