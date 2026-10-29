

using System;

namespace AzureTest.Application.Interfaces.Services;

public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    string? PhoneNumber { get; }
    string? ProfileImage { get; }
    string FirstName { get; }
    string LastName { get; }
    string FullName { get; }
    System.Collections.Generic.IReadOnlyList<string> Roles { get; }
    System.Collections.Generic.IReadOnlyList<string> DirectPermissions { get; }
    System.Collections.Generic.IReadOnlyList<string> RolePermissions { get; }
    System.Collections.Generic.IReadOnlyList<string> Permissions { get; }
    public bool IsAdmin { get; }
    public bool IsReadOnly { get; }
    public Guid TenantId { get; }

    /// <summary>
    /// Checks if the current user has access to the current request URL using the internally injected HttpContext.
    /// </summary>
    bool HasAccess();

    /// <summary>
    /// Checks if the current user has access to a specific URL using the internally injected HttpContext.
    /// </summary>
    bool HasAccess(string url);

    /// <summary>
    /// Checks if the current user has access to the URL being requested via the provided IHttpContextAccessor.
    /// Optionally override with a specific <paramref name="url"/>.
    /// </summary>
    bool HasAccess(Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor, string? url = null);

    /// <summary>
    /// Checks if the current user has been explicitly granted a direct permission override.
    /// </summary>
    bool HasDirectPermission(string permissionKey);

    /// <summary>
    /// Checks if the current user has a specific permission (either via direct user grant or assigned role).
    /// </summary>
    bool HasPermission(string permissionKey);

    bool BelongToCurrentUser(string entityUserId);
    bool BelongToCurrentUser(Guid entityUserId);
}
