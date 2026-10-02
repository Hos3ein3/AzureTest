

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
    System.Collections.Generic.IReadOnlyList<string> Permissions { get; }
}
