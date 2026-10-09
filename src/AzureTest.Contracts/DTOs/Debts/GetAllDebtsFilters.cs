using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Contracts.DTOs.Debts;

public class GetAllDebtsFilters : PagedFilterRequest
{
    public bool? IsDone { get; set; }

    public GetAllDebtsFilters()
    {
        SortBy = "DateOfBorrow";
        SortDescending = true;
    }
}
