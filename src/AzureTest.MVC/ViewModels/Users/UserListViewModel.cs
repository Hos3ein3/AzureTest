using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Users;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AzureTest.MVC.ViewModels.Users;

public class UserListViewModel
{
    public PagedResult<UserListDto> Users { get; set; } = null!;
    public string? SearchTerm { get; set; }
    public string? Role { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int PageSize { get; set; } = 10;
    
    public List<SelectListItem> AvailableRoles { get; set; } = new();
}
