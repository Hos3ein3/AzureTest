

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.ViewModels.Users;

public class UserCreateViewModel : IUserFormViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = null!;

    [Required]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = null!;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = null!;

    [Display(Name = "Roles")]
    public string[] SelectedRoles { get; set; } = System.Array.Empty<string>();

    public List<SelectListItem> AvailableRoles { get; set; } = new();
}

public interface IUserFormViewModel
{
    string FirstName { get; set; }
    string LastName { get; set; }
}
