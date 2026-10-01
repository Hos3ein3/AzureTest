

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? ProfileImage { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

