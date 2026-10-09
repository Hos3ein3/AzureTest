using System;
using AzureTest.Contracts.DTOs.Common;

namespace AzureTest.Contracts.DTOs.Expenses;

public class GetAllExpensesFilters : PagedFilterRequest
{
    public Guid? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public GetAllExpensesFilters()
    {
        SortBy = "Date";
        SortDescending = true;
    }
}
