using System;

namespace AzureTest.Contracts.DTOs.Expenses;

public interface IExpenseFormDto
{
    decimal Amount { get; set; }
    DateTime Date { get; set; }
    string Description { get; set; }
    Guid? CategoryId { get; set; }
    string? NewCategoryName { get; set; }
}
