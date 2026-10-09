

using System;

namespace AzureTest.Contracts.DTOs.Expenses;

public class CreateExpenseDto : IExpenseFormDto
{
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? CategoryId { get; set; }
    public string? NewCategoryName { get; set; }
}
