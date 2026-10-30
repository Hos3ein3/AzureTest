using System;

namespace AzureTest.Contracts.DTOs.Users;

public sealed record LoginRequestDto(
    string Email = "",
    string Password = "",
    bool RememberMe = true,
    string? ReturnUrl = null
);

public sealed record LoginResponseDto(
    bool Succeeded,
    bool IsLockedOut,
    string? Token,
    string? RefreshToken,
    string? RedirectUrl,
    string? Message = null,
    DateTimeOffset? LockoutEnd = null
);

public sealed record RegisterRequestDto(
    string Name = "",
    string Email = "",
    string Password = "",
    string ConfirmPassword = ""
);

public sealed record ForgotPasswordRequestDto(
    string Email = ""
);

public sealed record ResetPasswordRequestDto(
    string UserId = "",
    string Code = "",
    string Password = "",
    string ConfirmPassword = ""
);

public sealed record ChangePasswordRequestDto(
    string CurrentPassword = "",
    string NewPassword = "",
    string ConfirmPassword = ""
);

public sealed record RefreshTokenRequestDto(
    string Token = "",
    string RefreshToken = ""
);

public record AuthTokensDto(string? Token, string? RefreshToken);

public record AuthResultDto(bool Success, string[] Errors, string? Token, string? RefreshToken);
