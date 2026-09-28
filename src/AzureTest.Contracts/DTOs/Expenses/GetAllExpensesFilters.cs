namespace AzureTest.Contracts.DTOs.Expenses;

public class GetAllExpensesFilters
{
    public string? SearchTerm { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "Date";
    public bool SortDescending { get; set; } = true;
    public System.Guid? UserId { get; set; }
    public System.DateTime? StartDate { get; set; }
    public System.DateTime? EndDate { get; set; }
}
