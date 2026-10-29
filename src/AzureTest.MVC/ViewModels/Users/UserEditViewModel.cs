using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AzureTest.Domain.Documents.Audit;

namespace AzureTest.MVC.ViewModels.Users;

public class UserEditViewModel : IUserFormViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Email Address")]
    public string Email { get; set; } = null!;

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Read-Only Account")]
    public bool IsReadOnly { get; set; }

    // Account Security & Status Metadata
    public bool IsLockedOut { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public IReadOnlyList<string> LoginProviders { get; set; } = new List<string>();
    public IReadOnlyList<string> Roles { get; set; } = new List<string>();
    public IReadOnlyList<AuditLogDocument> AuditLogs { get; set; } = new List<AuditLogDocument>();
}
