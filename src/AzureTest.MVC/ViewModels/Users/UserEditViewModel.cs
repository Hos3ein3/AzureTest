

using System;
using System.ComponentModel.DataAnnotations;

namespace AzureTest.MVC.ViewModels.Users;

public class UserEditViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Email Address")]
    public string Email { get; set; } = null!; // Read-only in this form

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }
}
