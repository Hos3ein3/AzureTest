using System.ComponentModel.DataAnnotations;

namespace AzureTest.MVC.ViewModels.Roles;

public class RoleCreateViewModel
{
    [Required]
    [Display(Name = "Role Name")]
    [StringLength(50, MinimumLength = 2)]
    public string RoleName { get; set; } = null!;
}
