using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Users;

public sealed record UserListDto(
    Guid Id,
    string FullName,
    string Email,
    bool EmailConfirmed,
    bool IsLockedOut,
    IList<string> Roles
);
