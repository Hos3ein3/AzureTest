namespace AzureTest.Application.Users.Commands.LoginUser;

public record LoginUserResult(bool Success, string? Token, string? RefreshToken, string[]? Errors = null)
{
    public LoginUserResult(bool success, string? token, string[]? errors) 
        : this(success, token, null, errors) { }
}
