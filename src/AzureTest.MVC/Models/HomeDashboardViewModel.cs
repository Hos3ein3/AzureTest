using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Resumes;
using AzureTest.Contracts.DTOs.Trading;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Contracts.DTOs.Forex;

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

    // ── Universal Paper Trading Analytics ──────────────────────────────────
    public TradingAnalyticsSummaryModel? TradingAnalytics { get; set; }

    // ── Crypto Portfolio & Market Telemetry ────────────────────────────────
    public CryptoAnalyticsSummaryModel? CryptoAnalytics { get; set; }

    // ── Forex Trading & Global Sessions Analytics ──────────────────────────
    public ForexAnalyticsSummaryModel? ForexAnalytics { get; set; }
}

public class TradingAnalyticsSummaryModel
{
    public decimal CashBalance { get; set; }
    public decimal Equity { get; set; }
    public decimal RealizedPnl { get; set; }
    public decimal UnrealizedPnl { get; set; }
    public decimal MarginUsed { get; set; }
    public decimal FreeMargin { get; set; }
    public decimal MarginLevelPercent { get; set; }
    public decimal MarginUsedPercent { get; set; }
    public int OpenPositionsCount { get; set; }
    public int ClosedPositionsCount { get; set; }
    public int TotalTradesCount { get; set; }
    public int WinningTradesCount { get; set; }
    public int LosingTradesCount { get; set; }
    public int BreakEvenTradesCount { get; set; }
    public int LongTradesCount { get; set; }
    public int ShortTradesCount { get; set; }
    public decimal WinRatePercent { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal BestTradePnl { get; set; }
    public decimal WorstTradePnl { get; set; }
    public decimal AvgTradePnl { get; set; }
    public Dictionary<string, int> AssetClassDistribution { get; set; } = new();
    public Dictionary<string, decimal> EquityCurveHistory { get; set; } = new();
    public Dictionary<string, decimal> PnlBySymbol { get; set; } = new();
    public IReadOnlyList<TradingPositionDto> TopOpenPositions { get; set; } = new List<TradingPositionDto>();
    public IReadOnlyList<TradingQuoteDto> FeaturedQuotes { get; set; } = new List<TradingQuoteDto>();
}

public class CryptoAnalyticsSummaryModel
{
    public decimal TotalBalance { get; set; }
    public decimal Equity { get; set; }
    public decimal RealizedPnl { get; set; }
    public decimal UnrealizedPnl { get; set; }
    public decimal MarginUtilizationPercent { get; set; }
    public decimal MarginAvailablePercent { get; set; }
    public int OpenPositionsCount { get; set; }
    public int TotalTradesCount { get; set; }
    public int WinningTradesCount { get; set; }
    public int LosingTradesCount { get; set; }
    public int LongPositionsCount { get; set; }
    public int ShortPositionsCount { get; set; }
    public decimal WinRatePercent { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal SharpeRatio { get; set; }
    public decimal MaxDrawdownPercent { get; set; }
    public int FearAndGreedIndex { get; set; }
    public string FearAndGreedSentiment { get; set; } = "Neutral";
    public decimal BtcDominance { get; set; }
    public Dictionary<string, decimal> AssetAllocation { get; set; } = new();
    public Dictionary<string, decimal> CumulativePnlHistory { get; set; } = new();
    public Dictionary<string, decimal> TopMovers24h { get; set; } = new();
    public Dictionary<string, decimal> PnlByCoin { get; set; } = new();
    public IReadOnlyList<CryptoCoinDto> TopGainers { get; set; } = new List<CryptoCoinDto>();
    public IReadOnlyList<CryptoCoinDto> TopLosers { get; set; } = new List<CryptoCoinDto>();
}

public class ForexAnalyticsSummaryModel
{
    public decimal Balance { get; set; }
    public decimal Equity { get; set; }
    public decimal FreeMargin { get; set; }
    public decimal MarginLevelPercent { get; set; }
    public decimal MarginUsedPercent { get; set; }
    public decimal DailyPnl { get; set; }
    public decimal DailyDrawdownPercent { get; set; }
    public decimal DailyDrawdownLimitPercent { get; set; } = 5.0m;
    public decimal PeakEquity { get; set; }
    public int TotalTradesCount { get; set; }
    public int OpenTradesCount { get; set; }
    public int WinningTradesCount { get; set; }
    public int LosingTradesCount { get; set; }
    public decimal WinRatePercent { get; set; }
    public decimal ProfitFactor { get; set; }
    public int LongTradesCount { get; set; }
    public int ShortTradesCount { get; set; }
    public Dictionary<string, int> PairExposure { get; set; } = new();
    public Dictionary<string, decimal> PnlByPair { get; set; } = new();
    public Dictionary<string, decimal> EquityCurveHistory { get; set; } = new();
    public IReadOnlyList<ForexSessionDto> ActiveSessions { get; set; } = new List<ForexSessionDto>();
}
