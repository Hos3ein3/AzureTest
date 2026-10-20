using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Debts;
using AzureTest.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AzureTest.Application.Debts.Queries;

public sealed record GetDebtAnalyticsQuery(
    Guid? UserId = null,
    List<Guid>? UserIds = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null) : IQuery<DebtAnalyticsDto>;

public sealed class GetDebtAnalyticsQueryHandler : BaseQueryHandler<GetDebtAnalyticsQuery, DebtAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INoSqlUnitOfWork _noSqlUnitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetDebtAnalyticsQueryHandler(
        IUnitOfWork unitOfWork,
        INoSqlUnitOfWork noSqlUnitOfWork,
        ICurrentUser currentUser,
        ILogger<GetDebtAnalyticsQueryHandler> logger)
        : base(logger)
    {
        _unitOfWork = unitOfWork;
        _noSqlUnitOfWork = noSqlUnitOfWork;
        _currentUser = currentUser;
    }

    protected override async Task<DebtAnalyticsDto> ExecuteAsync(GetDebtAnalyticsQuery query, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUser.IsAdmin || _currentUser.Roles.Contains("Admin");
        var allDebts = (await _unitOfWork.Debts.FindAsync(r => !r.IsDeleted)).ToList();

        // Scoping
        if (!isAdmin)
        {
            allDebts = allDebts.Where(d => d.UserId == _currentUser.Id).ToList();
        }
        else
        {
            if (query.UserIds != null && query.UserIds.Any())
            {
                var set = new HashSet<Guid>(query.UserIds);
                allDebts = allDebts.Where(d => set.Contains(d.UserId)).ToList();
            }
            else if (query.UserId.HasValue)
            {
                allDebts = allDebts.Where(d => d.UserId == query.UserId.Value).ToList();
            }
        }

        // Optional date filter
        if (query.FromDate.HasValue)
        {
            allDebts = allDebts.Where(d => d.DateOfBorrow >= query.FromDate.Value || d.CreatedOn >= query.FromDate.Value).ToList();
        }
        if (query.ToDate.HasValue)
        {
            var toDate = query.ToDate.Value.TimeOfDay == TimeSpan.Zero
                ? query.ToDate.Value.Date.AddDays(1).AddTicks(-1)
                : query.ToDate.Value;
            allDebts = allDebts.Where(d => d.DateOfBorrow <= toDate || d.CreatedOn <= toDate).ToList();
        }

        // Preload Person names
        var personIds = allDebts.Select(d => d.DebtPersonId).Distinct().ToList();
        var personNames = new Dictionary<Guid, string>();
        foreach (var pid in personIds)
        {
            var p = await _noSqlUnitOfWork.DebtPersons.GetByIdAsync(pid);
            if (p != null) personNames[pid] = p.FullName;
        }

        var today = DateTime.Today;

        // Map DTOs
        var dtos = allDebts.Select(d => new DebtDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Amount = d.Amount,
            DateOfBorrow = d.DateOfBorrow,
            DateOfReturn = d.DateOfReturn,
            PromissedReturnDate = d.PromissedReturnDate,
            Profit = d.Profit,
            DebtPersonId = d.DebtPersonId,
            DebtPersonName = personNames.TryGetValue(d.DebtPersonId, out var name) ? name : "Unknown",
            Description = d.Description,
            IsDone = d.IsDone,
            CreatedOn = d.CreatedOn
        }).ToList();

        decimal totalBorrowed = allDebts.Sum(d => d.Amount);
        decimal totalReturned = allDebts.Where(d => d.IsDone).Sum(d => d.Amount);
        decimal totalOutstanding = allDebts.Where(d => !d.IsDone).Sum(d => d.Amount);
        decimal totalProfit = allDebts.Sum(d => d.Profit);

        var overdueDebts = allDebts
            .Where(d => !d.IsDone && d.PromissedReturnDate.HasValue && d.PromissedReturnDate.Value.Date < today)
            .ToList();
        int overdueCount = overdueDebts.Count;
        decimal overdueAmount = overdueDebts.Sum(d => d.Amount);

        int totalCount = allDebts.Count;
        int settledCount = allDebts.Count(d => d.IsDone);
        int pendingCount = allDebts.Count(d => !d.IsDone);

        // Person Breakdown
        var personBreakdown = allDebts
            .GroupBy(d => d.DebtPersonId)
            .Select(g =>
            {
                var pBorrowed = g.Sum(x => x.Amount);
                var pReturned = g.Where(x => x.IsDone).Sum(x => x.Amount);
                var pOutstanding = g.Where(x => !x.IsDone).Sum(x => x.Amount);
                var pProfit = g.Sum(x => x.Profit);
                var pTotalCount = g.Count();
                var pPending = g.Count(x => !x.IsDone);
                var pOverdue = g.Count(x => !x.IsDone && x.PromissedReturnDate.HasValue && x.PromissedReturnDate.Value.Date < today);
                var lastDate = g.Max(x => (DateTime?)x.DateOfBorrow);
                var pName = personNames.TryGetValue(g.Key, out var val) ? val : "Unknown";

                return new DebtPersonBreakdownDto(
                    g.Key,
                    pName,
                    pBorrowed,
                    pReturned,
                    pOutstanding,
                    pProfit,
                    pTotalCount,
                    pPending,
                    pOverdue,
                    lastDate);
            })
            .OrderByDescending(p => p.TotalOutstanding)
            .ThenByDescending(p => p.TotalBorrowed)
            .ToList();

        // Status Distribution
        var onTrackPendingAmount = allDebts
            .Where(d => !d.IsDone && (!d.PromissedReturnDate.HasValue || d.PromissedReturnDate.Value.Date >= today))
            .Sum(d => d.Amount);
        var onTrackPendingCount = allDebts
            .Count(d => !d.IsDone && (!d.PromissedReturnDate.HasValue || d.PromissedReturnDate.Value.Date >= today));

        var statusDistribution = new List<DebtStatusPointDto>();
        if (totalCount > 0)
        {
            statusDistribution.Add(new DebtStatusPointDto(
                "Settled",
                settledCount,
                totalReturned,
                Math.Round((double)settledCount / totalCount * 100, 1)));

            statusDistribution.Add(new DebtStatusPointDto(
                "Active / On Track",
                onTrackPendingCount,
                onTrackPendingAmount,
                Math.Round((double)onTrackPendingCount / totalCount * 100, 1)));

            statusDistribution.Add(new DebtStatusPointDto(
                "Overdue",
                overdueCount,
                overdueAmount,
                Math.Round((double)overdueCount / totalCount * 100, 1)));
        }

        // Monthly Trend (last 6 months or all active)
        var monthlyTrend = allDebts
            .GroupBy(d => new { d.DateOfBorrow.Year, d.DateOfBorrow.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new DebtMonthlyPointDto(
                $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                g.Sum(x => x.Amount),
                g.Where(x => x.IsDone).Sum(x => x.Amount),
                g.Sum(x => x.Profit)))
            .ToList();

        var recentDebts = dtos.OrderByDescending(d => d.DateOfBorrow).Take(8).ToList();
        var overdueDtos = dtos.Where(d => !d.IsDone && d.PromissedReturnDate.HasValue && d.PromissedReturnDate.Value.Date < today)
            .OrderBy(d => d.PromissedReturnDate)
            .ToList();

        return new DebtAnalyticsDto(
            totalBorrowed,
            totalReturned,
            totalOutstanding,
            totalProfit,
            overdueCount,
            overdueAmount,
            totalCount,
            settledCount,
            pendingCount,
            personBreakdown,
            statusDistribution,
            monthlyTrend,
            recentDebts,
            overdueDtos);
    }
}
