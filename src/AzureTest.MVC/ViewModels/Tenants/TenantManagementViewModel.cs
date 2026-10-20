using System;
using System.Collections.Generic;

namespace AzureTest.MVC.ViewModels.Tenants;

public class TenantManagementViewModel
{
    public bool IsMultiTenancyEnabled { get; set; }
    public Guid CurrentTenantId { get; set; }
    public Guid RealAdminUserId { get; set; }
    public bool IsAdminImpersonating { get; set; }
    public string? CurrentTenantName { get; set; }
    public string? CurrentTenantEmail { get; set; }

    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / (PageSize > 0 ? PageSize : 10));
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public List<TenantItemViewModel> Tenants { get; set; } = new();
}

public class TenantItemViewModel
{
    public Guid TenantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool IsActiveTenant { get; set; }
    public bool IsCurrentAdmin { get; set; }
}
