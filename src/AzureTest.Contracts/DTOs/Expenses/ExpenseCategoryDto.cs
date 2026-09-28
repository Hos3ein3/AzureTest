using System;

namespace AzureTest.Contracts.DTOs.Expenses;

public class ExpenseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
