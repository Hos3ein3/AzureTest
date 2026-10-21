

using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Resumes;

namespace AzureTest.MVC.Models;

public class HomeDashboardViewModel
{
    public ResumeAnalyticsDto Analytics { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public int OnlineUsersCount { get; set; }
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

    // Pages Widget
    public IReadOnlyList<AzureTest.Contracts.DTOs.Pages.PageDto> RecentPages { get; set; } = new List<AzureTest.Contracts.DTOs.Pages.PageDto>();
    public int UnreadPagesCount { get; set; }

    // Media Controller Analytics
    public AzureTest.Contracts.DTOs.Media.MediaStatsDto? MediaStats { get; set; }
    public Dictionary<string, int> MediaByStatus { get; set; } = new();
    public Dictionary<string, int> MediaByType { get; set; } = new();
    public int TotalMediaHours { get; set; }

    // Health, Weight & BMI Analytics
    public AzureTest.Contracts.DTOs.Gym.WorkoutAnalyticsDto? HealthStats { get; set; }
    public double? CurrentWeightKg { get; set; }
    public double? CurrentBmi { get; set; }
    public string CurrentBmiCategory { get; set; } = "Not Recorded";
    public Dictionary<string, double> WeightHistoryTrend { get; set; } = new();
    public Dictionary<string, double> BodyComposition { get; set; } = new();

    // Shopping List Analytics
    public int ShoppingListTotalItems { get; set; }
    public int ShoppingListToBuyItems { get; set; }
    public int ShoppingListBoughtItems { get; set; }
    public int ShoppingListInCartItems { get; set; }
    public decimal ShoppingListEstimatedCost { get; set; }
    public Dictionary<string, int> ShoppingListByCategory { get; set; } = new();
}
