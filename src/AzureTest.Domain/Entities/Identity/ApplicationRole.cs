using System;
using Microsoft.AspNetCore.Identity;

namespace AzureTest.Domain.Entities.Identity;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }

    // Role priority to determine hierarchy (e.g. SEO Specialist > Writer)
    public int Priority { get; set; }
}
