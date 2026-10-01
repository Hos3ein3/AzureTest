

using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.MVC.Models;

public class HomeDashboardViewModel
{
    public ResumeAnalyticsDto Analytics { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;
    public string CurrentUserName { get; set; } = string.Empty;
    public Dictionary<string, string> AvailableUsers { get; set; } = new();

    public IReadOnlyList<AzureTest.Contracts.DTOs.Events.EventDto> RecentEvents { get; set; } = new List<AzureTest.Contracts.DTOs.Events.EventDto>();
    public IReadOnlyList<AzureTest.Contracts.DTOs.Tasks.TaskDto> PendingTasks { get; set; } = new List<AzureTest.Contracts.DTOs.Tasks.TaskDto>();
    public IReadOnlyList<AzureTest.Contracts.DTOs.Tickets.TicketDto> Tickets { get; set; } = new List<AzureTest.Contracts.DTOs.Tickets.TicketDto>();
    public IReadOnlyList<AzureTest.Contracts.DTOs.Habits.HabitTrackerDto> RecentHabits { get; set; } = new List<AzureTest.Contracts.DTOs.Habits.HabitTrackerDto>();
    public IReadOnlyList<AzureTest.Contracts.DTOs.Debts.DebtDto> RecentDebts { get; set; } = new List<AzureTest.Contracts.DTOs.Debts.DebtDto>();
    public IReadOnlyList<AzureTest.Contracts.DTOs.Debts.DebtDto> PromissedDebts { get; set; } = new List<AzureTest.Contracts.DTOs.Debts.DebtDto>();
    public IReadOnlyList<AzureTest.Contracts.DTOs.Expenses.ExpenseDto> RecentExpenses { get; set; } = new List<AzureTest.Contracts.DTOs.Expenses.ExpenseDto>();
    
    public Dictionary<string, decimal> ExpensesByCategory { get; set; } = new();
    public Dictionary<string, decimal> DebtsByPerson { get; set; } = new();
    
    // New Graph Data
    public Dictionary<string, int> HabitAdherence { get; set; } = new();
    public Dictionary<string, int> TicketStatusCounts { get; set; } = new();
    public Dictionary<string, decimal> MonthlyExpenses { get; set; } = new();
    public Dictionary<string, decimal> MonthlyDebts { get; set; } = new();
    
    public bool IsViewingOtherUser { get; set; }
    
    public decimal TotalOutstandingDebt { get; set; }
    public decimal TotalExpensesThisMonth { get; set; }
}
