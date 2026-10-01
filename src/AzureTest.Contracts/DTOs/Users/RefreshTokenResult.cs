

namespace AzureTest.Contracts.DTOs.Users;

public record RefreshTokenResult(bool Success, string[]? Errors, string? Token, string? RefreshToken);
