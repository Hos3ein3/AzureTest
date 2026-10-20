using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Contracts.DTOs.Debts;

public class GetAllDebtsFilters : PagedFilterRequest
{
    public bool? IsDone { get; set; }
    public Guid? DebtPersonId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    public GetAllDebtsFilters()
    {
        SortBy = "DateOfBorrow";
        SortDescending = true;
    }
}
