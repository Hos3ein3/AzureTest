using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.ViewModels.Users;

public class UserRolesViewModel
{
    public Guid UserId { get; set; }
    
    public string FullName { get; set; } = null!;
    
    public string Email { get; set; } = null!;

    [Display(Name = "Roles")]
    public string[] SelectedRoles { get; set; } = Array.Empty<string>();

    public List<SelectListItem> AvailableRoles { get; set; } = new();
}
