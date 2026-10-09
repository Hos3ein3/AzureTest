using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Contracts.DTOs.Users;

public class GetAllUsersFilters : PagedFilterRequest
{
    public string? Role { get; set; }

    public GetAllUsersFilters()
    {
        SortBy = "FullName";
        SortDescending = false;
    }
}
