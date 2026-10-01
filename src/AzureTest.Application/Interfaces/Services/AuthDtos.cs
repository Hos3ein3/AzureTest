using AzureTest.Application.Abstractions;


using System;

namespace AzureTest.Application.Interfaces.Services;

public record AuthTokensDto(string? Token, string? RefreshToken);

public record AuthResultDto(bool Success, string[] Errors, string? Token, string? RefreshToken);
