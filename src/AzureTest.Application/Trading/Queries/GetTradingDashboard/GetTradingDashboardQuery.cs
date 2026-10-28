using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Application.Trading.Services;
using AzureTest.Contracts.DTOs.Trading;
using AzureTest.Domain.Entities.Trading;
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Application.Trading.Queries.GetTradingDashboard;

public sealed record GetTradingDashboardQuery(
    Guid UserId,
    Guid TenantId,
    string? SelectedSymbol = null) : IQuery<Result<TradingDashboardDto>>
{
    public static class Mapping
    {
        public static TradingAccountDto ToDto(TradingAccount a) =>
            new(a.Id, a.UserId, a.TenantId, a.Name, a.Currency, a.CashBalance, a.Equity, a.MarginUsed, a.FreeMargin, a.RealizedPnl);

        public static TradingPositionDto ToDto(TradingPosition p) =>
            new(p.Id, p.AccountId, p.Symbol, p.AssetName, p.AssetClass, p.Side, p.Quantity, p.EntryPrice, p.CurrentPrice, p.ClosePrice, p.Leverage, p.InitialMargin, p.UnrealizedPnl, p.RealizedPnl, p.StopLoss, p.TakeProfit, p.MultipleTakeProfitsJson, p.TrailingStopDistance, p.Status, p.OpenedAtUtc, p.ClosedAtUtc);

        public static TradingOrderDto ToDto(TradingOrder o) =>
            new(o.Id, o.AccountId, o.Symbol, o.AssetName, o.AssetClass, o.Side, o.OrderType, o.Quantity, o.Price, o.TriggerPrice, o.Leverage, o.StopLoss, o.TakeProfit, o.MultipleTakeProfitsJson, o.TimeInForce, o.Status, o.PlacedAtUtc, o.FilledAtUtc, o.CancelledAtUtc);

        public static TradingWatchlistDto ToDto(TradingWatchlist w) =>
            new(w.Id, w.Symbol, w.AssetName, w.AssetClass, w.AddedAtUtc);
    }
}

public sealed class GetTradingDashboardQueryHandler : IQueryHandler<GetTradingDashboardQuery, Result<TradingDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;

    public GetTradingDashboardQueryHandler(IUnitOfWork uow, ITradingMarketDataService marketData)
    {
        _uow = uow;
        _marketData = marketData;
    }

    public async Task<Result<TradingDashboardDto>> HandleAsync(GetTradingDashboardQuery q, CancellationToken ct)
    {
        var accounts = await _uow.TradingAccounts.GetAllAsync();
        var account = accounts.FirstOrDefault(a => a.UserId == q.UserId && (q.TenantId == Guid.Empty || a.TenantId == q.TenantId));
        if (account == null)
        {
            account = TradingAccount.Create(q.UserId, q.TenantId, "Primary Paper Trading Account");
            _uow.TradingAccounts.Add(account);
            await _uow.SaveChangesAsync(ct);
        }

        var allPositions = (await _uow.TradingPositions.GetAllAsync())
            .Where(p => p.UserId == q.UserId && (q.TenantId == Guid.Empty || p.TenantId == q.TenantId))
            .OrderByDescending(p => p.OpenedAtUtc)
            .ToList();

        // Update current prices and unrealized pnl for open positions
        await _marketData.RefreshRealWorldDataAsync(ct);
        decimal totalUnrealizedPnl = 0m;
        foreach (var pos in allPositions.Where(p => p.Status == TradingPositionStatus.Open))
        {
            var curPrice = await _marketData.GetCurrentPriceAsync(pos.Symbol, pos.CurrentPrice, ct);
            pos.UpdateCurrentPrice(curPrice);
            totalUnrealizedPnl += pos.UnrealizedPnl;
        }

        account.AdjustMarginAndEquity(0, totalUnrealizedPnl);

        var openPositions = allPositions
            .Where(p => p.Status == TradingPositionStatus.Open)
            .Select(GetTradingDashboardQuery.Mapping.ToDto)
            .ToList();

        var closedPositions = allPositions
            .Where(p => p.Status == TradingPositionStatus.Closed || p.Status == TradingPositionStatus.Liquidated)
            .Take(25)
            .Select(GetTradingDashboardQuery.Mapping.ToDto)
            .ToList();

        var pendingOrders = (await _uow.TradingOrders.GetAllAsync())
            .Where(o => o.UserId == q.UserId && (q.TenantId == Guid.Empty || o.TenantId == q.TenantId) && o.Status == TradingOrderStatus.Pending)
            .OrderByDescending(o => o.PlacedAtUtc)
            .Select(GetTradingDashboardQuery.Mapping.ToDto)
            .ToList();

        var watchlistEntities = (await _uow.TradingWatchlists.GetAllAsync())
            .Where(w => w.UserId == q.UserId && (q.TenantId == Guid.Empty || w.TenantId == q.TenantId))
            .OrderBy(w => w.Symbol)
            .ToList();

        var watchlistedSymbols = new HashSet<string>(watchlistEntities.Select(w => w.Symbol), StringComparer.OrdinalIgnoreCase);
        var watchlist = watchlistEntities.Select(GetTradingDashboardQuery.Mapping.ToDto).ToList();

        var quotes = await _marketData.GetAllQuotesAsync(watchlistedSymbols, ct);

        var selectedSymbol = string.IsNullOrWhiteSpace(q.SelectedSymbol) ? "BTCUSDT" : q.SelectedSymbol.Trim().ToUpperInvariant();
        var selectedQuote = quotes.FirstOrDefault(x => x.Symbol.Equals(selectedSymbol, StringComparison.OrdinalIgnoreCase)) ?? quotes.First();

        var dto = new TradingDashboardDto(
            GetTradingDashboardQuery.Mapping.ToDto(account),
            openPositions,
            closedPositions,
            pendingOrders,
            watchlist,
            quotes,
            selectedQuote);

        return Result<TradingDashboardDto>.Success(dto);
    }
}
