using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Application.Trading.Queries.GetTradingHistory;

public sealed record GetTradingHistoryQuery(
    Guid UserId,
    Guid TenantId,
    string? Search = null,
    TradingOrderSide? Side = null,
    AssetClass? AssetClass = null,
    string? Outcome = null
) : IQuery<Result<TradingHistoryDto>>;

public sealed class GetTradingHistoryQueryHandler : IQueryHandler<GetTradingHistoryQuery, Result<TradingHistoryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITenantProvider _tenantProvider;

    public GetTradingHistoryQueryHandler(IUnitOfWork uow, ITenantProvider tenantProvider)
    {
        _uow = uow;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<TradingHistoryDto>> HandleAsync(GetTradingHistoryQuery q, CancellationToken ct)
    {
        var isReadOnlyImpersonation = _tenantProvider.IsAdminImpersonating && !_tenantProvider.AllowImpersonationWrites;

        var accounts = await _uow.TradingAccounts.GetAllAsync();
        var account = accounts.FirstOrDefault(a => a.UserId == q.UserId && (q.TenantId == Guid.Empty || a.TenantId == q.TenantId));
        if (account == null)
        {
            account = TradingAccount.Create(q.UserId, q.TenantId, "Primary Paper Trading Account");
            if (!isReadOnlyImpersonation)
            {
                _uow.TradingAccounts.Add(account);
                await _uow.SaveChangesAsync(ct);
            }
        }

        var accountDto = new TradingAccountDto(
            account.Id,
            account.UserId,
            account.TenantId,
            account.Name,
            account.Currency,
            account.CashBalance,
            account.Equity,
            account.MarginUsed,
            account.FreeMargin,
            account.RealizedPnl);

        var allClosed = (await _uow.TradingPositions.GetAllAsync())
            .Where(p => p.UserId == q.UserId && (q.TenantId == Guid.Empty || p.TenantId == q.TenantId) &&
                        (p.Status == TradingPositionStatus.Closed || p.Status == TradingPositionStatus.Liquidated))
            .OrderByDescending(p => p.ClosedAtUtc ?? p.OpenedAtUtc)
            .ToList();

        // Summary metrics calculated over all closed trades before filtering
        var totalClosedTrades = allClosed.Count;
        var winningTradesAll = allClosed.Where(p => p.RealizedPnl > 0).ToList();
        var losingTradesAll = allClosed.Where(p => p.RealizedPnl < 0).ToList();
        var beTradesAll = allClosed.Where(p => p.RealizedPnl == 0).ToList();

        var winCount = winningTradesAll.Count;
        var lossCount = losingTradesAll.Count;
        var beCount = beTradesAll.Count;
        var winRate = totalClosedTrades > 0 ? Math.Round(((decimal)winCount / totalClosedTrades) * 100m, 2) : 0m;

        var totalGains = winningTradesAll.Sum(p => p.RealizedPnl);
        var totalLosses = Math.Abs(losingTradesAll.Sum(p => p.RealizedPnl));
        var netPnl = allClosed.Sum(p => p.RealizedPnl);
        var profitFactor = totalLosses > 0 ? Math.Round(totalGains / totalLosses, 2) : (totalGains > 0 ? 99.9m : 0m);

        var avgWin = winCount > 0 ? Math.Round(totalGains / winCount, 2) : 0m;
        var avgLoss = lossCount > 0 ? Math.Round(totalLosses / lossCount, 2) : 0m;
        var payoffRatio = avgLoss > 0 ? Math.Round(avgWin / avgLoss, 2) : (avgWin > 0 ? 99.9m : 0m);

        var largestWin = allClosed.Count > 0 ? allClosed.Max(p => p.RealizedPnl) : 0m;
        var largestLoss = allClosed.Count > 0 ? allClosed.Min(p => p.RealizedPnl) : 0m;
        var totalNotional = allClosed.Sum(p => p.Quantity * p.EntryPrice);

        var summary = new TradingHistorySummaryDto(
            totalClosedTrades,
            winCount,
            lossCount,
            beCount,
            winRate,
            totalGains,
            totalLosses,
            netPnl,
            profitFactor,
            avgWin,
            avgLoss,
            payoffRatio,
            largestWin,
            largestLoss,
            Math.Round(totalNotional, 2));

        // Apply filters
        var filteredQuery = allClosed.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim();
            filteredQuery = filteredQuery.Where(p =>
                p.Symbol.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.AssetName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (q.Side.HasValue)
        {
            filteredQuery = filteredQuery.Where(p => p.Side == q.Side.Value);
        }

        if (q.AssetClass.HasValue)
        {
            filteredQuery = filteredQuery.Where(p => p.AssetClass == q.AssetClass.Value);
        }

        if (!string.IsNullOrWhiteSpace(q.Outcome))
        {
            var outc = q.Outcome.Trim().ToLowerInvariant();
            filteredQuery = outc switch
            {
                "win" => filteredQuery.Where(p => p.RealizedPnl > 0),
                "loss" => filteredQuery.Where(p => p.RealizedPnl < 0),
                "breakeven" => filteredQuery.Where(p => p.RealizedPnl == 0),
                "liquidated" => filteredQuery.Where(p => p.Status == TradingPositionStatus.Liquidated),
                _ => filteredQuery
            };
        }

        var tradeItems = filteredQuery.Select(p =>
        {
            var notional = Math.Round(p.Quantity * p.EntryPrice, 4);
            var balancePct = account.Equity > 0
                ? Math.Round((p.InitialMargin / account.Equity) * 100m, 2)
                : 0m;
            var roiPct = p.InitialMargin > 0
                ? Math.Round((p.RealizedPnl / p.InitialMargin) * 100m, 2)
                : 0m;

            string outcomeBadge;
            string closeReasonBadge;

            if (p.Status == TradingPositionStatus.Liquidated)
            {
                outcomeBadge = "LIQUIDATED";
                closeReasonBadge = "LIQUIDATION";
            }
            else if (p.RealizedPnl > 0)
            {
                outcomeBadge = "WIN";
                var hitTp = false;
                if (p.TakeProfit.HasValue && p.ClosePrice.HasValue)
                {
                    if (p.Side == TradingOrderSide.Buy && p.ClosePrice.Value >= p.TakeProfit.Value * 0.999m) hitTp = true;
                    if (p.Side == TradingOrderSide.Sell && p.ClosePrice.Value <= p.TakeProfit.Value * 1.001m) hitTp = true;
                }
                closeReasonBadge = hitTp ? "TAKE PROFIT" : "MANUAL EXIT";
            }
            else if (p.RealizedPnl < 0)
            {
                outcomeBadge = "LOSS";
                var hitSl = false;
                if (p.StopLoss.HasValue && p.ClosePrice.HasValue)
                {
                    if (p.Side == TradingOrderSide.Buy && p.ClosePrice.Value <= p.StopLoss.Value * 1.001m) hitSl = true;
                    if (p.Side == TradingOrderSide.Sell && p.ClosePrice.Value >= p.StopLoss.Value * 0.999m) hitSl = true;
                }
                closeReasonBadge = hitSl ? "STOP LOSS" : "MANUAL EXIT";
            }
            else
            {
                outcomeBadge = "BREAK-EVEN";
                closeReasonBadge = "B/E EXIT";
            }

            var duration = p.ClosedAtUtc.HasValue ? (p.ClosedAtUtc.Value - p.OpenedAtUtc) : (TimeSpan?)null;

            return new TradingHistoryItemDto(
                p.Id,
                p.AccountId,
                p.Symbol,
                p.AssetName,
                p.AssetClass,
                p.Side,
                p.Quantity,
                p.EntryPrice,
                p.ClosePrice ?? p.CurrentPrice,
                p.CurrentPrice,
                p.Leverage,
                p.InitialMargin,
                account.CashBalance,
                balancePct,
                notional,
                p.StopLoss,
                p.TakeProfit,
                p.MultipleTakeProfitsJson,
                p.TrailingStopDistance,
                p.RealizedPnl,
                roiPct,
                p.Status,
                outcomeBadge,
                closeReasonBadge,
                p.OpenedAtUtc,
                p.ClosedAtUtc,
                duration);
        }).ToList();

        var dto = new TradingHistoryDto(
            accountDto,
            summary,
            tradeItems,
            q.Search,
            q.Side,
            q.AssetClass,
            q.Outcome);

        return Result<TradingHistoryDto>.Success(dto);
    }
}
