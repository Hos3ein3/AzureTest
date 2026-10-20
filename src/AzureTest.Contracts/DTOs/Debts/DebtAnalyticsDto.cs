using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.Debts;

public sealed record DebtAnalyticsDto(
    decimal TotalBorrowed,
    decimal TotalReturned,
    decimal TotalOutstanding,
    decimal TotalProfit,
    int OverdueDebtsCount,
    decimal OverdueDebtsAmount,
    int TotalDebtsCount,
    int SettledDebtsCount,
    int PendingDebtsCount,
    IReadOnlyList<DebtPersonBreakdownDto> PersonBreakdown,
    IReadOnlyList<DebtStatusPointDto> StatusDistribution,
    IReadOnlyList<DebtMonthlyPointDto> MonthlyTrend,
    IReadOnlyList<DebtDto> RecentDebts,
    IReadOnlyList<DebtDto> OverdueDebts);

public sealed record DebtPersonBreakdownDto(
    Guid PersonId,
    string PersonName,
    decimal TotalBorrowed,
    decimal TotalReturned,
    decimal TotalOutstanding,
    decimal TotalProfit,
    int TotalDebtsCount,
    int PendingCount,
    int OverdueCount,
    DateTime? LastBorrowDate);

public sealed record DebtStatusPointDto(
    string Status,
    int Count,
    decimal Amount,
    double Percentage);

public sealed record DebtMonthlyPointDto(
    string MonthLabel,
    decimal BorrowedAmount,
    decimal ReturnedAmount,
    decimal ProfitAmount);
