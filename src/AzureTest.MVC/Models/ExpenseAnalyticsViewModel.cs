

using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Expenses;

namespace AzureTest.MVC.Models;

public class ExpenseAnalyticsViewModel
{
    // Month over Month
    public decimal TotalThisMonth { get; set; }
    public decimal TotalLastMonth { get; set; }
    public double MonthOverMonthChangePercentage { get; set; }

    // By Category
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();

    // By Month
    public Dictionary<string, decimal> ExpensesByMonth { get; set; } = new();

    // Detailed Breakdown
    public List<ExpenseMonthGroup> MonthlyDetails { get; set; } = new();
}

public class ExpenseMonthGroup
{
    public string MonthName { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public List<ExpenseDto> Expenses { get; set; } = new();
}
