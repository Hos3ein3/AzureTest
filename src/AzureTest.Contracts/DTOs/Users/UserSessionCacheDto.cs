using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Users;

public class UserSessionCacheDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfileImage { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> DirectPermissions { get; set; } = new();
    public List<string> RolePermissions { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public bool IsReadOnly { get; set; } = false;
}
