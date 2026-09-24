using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Services;

public interface IUserManagerService
{
    Task<(bool Success, string[] Errors)> RegisterUserAsync(string email, string password, string firstName, string lastName);
    Task<bool> CheckPasswordAsync(string email, string password);
    Task<AuthTokensDto> GenerateJwtTokenAsync(string email);
    Task<AuthResultDto> RefreshTokenAsync(string token, string refreshToken);
    Task<(bool Success, string[] Errors)> AddUserToRoleAsync(string email, string roleName);
}
