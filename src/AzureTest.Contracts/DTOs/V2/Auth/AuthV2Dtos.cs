using System;

namespace AzureTest.Contracts.DTOs.V2.Auth;

/// <summary>
/// Version 2 DTOs for Auth feature endpoints.
/// </summary>
public sealed record LoginRequestV2Dto(
    string Email = "",
    string Password = "",
    bool RememberMe = true,
    string? DeviceId = null,
    string? ReturnUrl = null
);

public sealed record AuthTokensV2Dto(
    string? AccessToken,
    string? RefreshToken,
    long ExpiresInSeconds,
    string TokenType = "Bearer"
);
