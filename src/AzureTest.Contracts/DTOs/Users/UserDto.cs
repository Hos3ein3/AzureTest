

using System;

namespace AzureTest.Contracts.DTOs.Users;

public sealed record UserDto(
    Guid Id,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsLockedOut,
    DateTimeOffset? LockoutEnd,
    string Roles,
    string? ProfileImage
);
