

namespace AzureTest.Contracts.DTOs.Users;

public record LoginUserResult(bool Success, string? Token, string? RefreshToken, string[]? Errors = null)
{
    public LoginUserResult(bool success, string? token, string[]? errors) 
        : this(success, token, null, errors) { }
}
