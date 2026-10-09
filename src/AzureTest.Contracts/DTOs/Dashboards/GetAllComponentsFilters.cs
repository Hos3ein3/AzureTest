using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Contracts.DTOs.Dashboards;

public class GetAllComponentsFilters : PagedFilterRequest
{
    public string? Category { get; set; }
    public string? Type { get; set; }

    public GetAllComponentsFilters()
    {
        SortBy = "Name";
        SortDescending = false;
    }
}
