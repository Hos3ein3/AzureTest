using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexTradeDashboard;

public sealed record GetForexTradeDashboardQuery(ForexTradeFilter Filter) : IQuery<Result<TradeTerminalDashboardDto>>;

public sealed class GetForexTradeDashboardQueryValidator : AbstractValidator<GetForexTradeDashboardQuery>
{
    public GetForexTradeDashboardQueryValidator()
    {
        RuleFor(x => x.Filter.Page).GreaterThan(0);
        RuleFor(x => x.Filter.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetForexTradeDashboardQueryHandler : IQueryHandler<GetForexTradeDashboardQuery, Result<TradeTerminalDashboardDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IForexMarketDataService _marketData;
    private readonly ICurrentUser _currentUser;

    public GetForexTradeDashboardQueryHandler(
        IUnitOfWork unitOfWork,
        IForexMarketDataService marketData,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _marketData = marketData;
        _currentUser = currentUser;
    }

    public async Task<Result<TradeTerminalDashboardDto>> HandleAsync(GetForexTradeDashboardQuery query, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.ForexAccounts.FindAsync(a => a.UserId == _currentUser.Id);
        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            account = ForexAccount.Create(_currentUser.Id, "Demo Primary", "USD", 10_000m, 30, true);
            _unitOfWork.ForexAccounts.Add(account);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var openTradesEntities = await _unitOfWork.ForexTrades.FindAsync(
            t => t.UserId == _currentUser.Id && t.Status == ForexTradeStatus.Open);

        var pendingOrdersEntities = await _unitOfWork.ForexOrders.FindAsync(
            o => o.UserId == _currentUser.Id && o.Status == ForexOrderStatus.Pending);

        var recentClosedEntities = await _unitOfWork.ForexTrades.FindAsync(
            t => t.UserId == _currentUser.Id && t.Status == ForexTradeStatus.Closed);

        var overview = await _marketData.GetMarketOverviewAsync(new ForexMarketFilter(), cancellationToken);
        var quotes = overview.Quotes.ToDictionary(q => q.Symbol, StringComparer.OrdinalIgnoreCase);

        decimal totalUnrealizedPnl = 0m;
        decimal totalUsedMargin = 0m;

        var openTradeDtos = new List<ForexTradeDto>();
        foreach (var t in openTradesEntities)
        {
            quotes.TryGetValue(t.Symbol, out var quote);
            var currentPrice = quote != null
                ? (t.Direction == ForexTradeDirection.Buy ? quote.Bid : quote.Ask)
                : t.OpenPrice;

            var isJpy = t.Symbol.Contains("JPY", StringComparison.OrdinalIgnoreCase);
            var isGold = t.Symbol.Contains("XAU", StringComparison.OrdinalIgnoreCase);
            var pipMultiplier = isJpy ? 0.01m : (isGold ? 0.1m : 0.0001m);
            var pipValueStandardLot = isJpy ? 6.55m : 10.00m;

            var unrealizedPips = t.Direction == ForexTradeDirection.Buy
                ? (currentPrice - t.OpenPrice) / pipMultiplier
                : (t.OpenPrice - currentPrice) / pipMultiplier;

            var unrealizedPnl = Math.Round(unrealizedPips * pipValueStandardLot * t.LotSize, 2);
            totalUnrealizedPnl += unrealizedPnl;
            totalUsedMargin += t.MarginRequired;

            openTradeDtos.Add(new ForexTradeDto(
                t.Id, t.AccountId, t.OrderId, t.Symbol, t.Direction, t.LotSize, t.OpenPrice,
                t.ClosePrice, t.StopLoss, t.TakeProfit, t.TrailingStopPips, t.BreakEvenTriggered,
                t.MarginRequired, t.SwapCost, t.Commission, t.GrossProfit, t.NetProfit, t.Pips,
                t.Status, t.ExitReason, t.OpenTimeUtc, t.CloseTimeUtc, t.Notes, t.ScreenshotUrl,
                currentPrice, unrealizedPnl, Math.Round(unrealizedPips, 1)
            ));
        }

        account.UpdateMetrics(totalUnrealizedPnl, totalUsedMargin);

        var dailyPnl = account.Balance - account.DailyStartingBalance + totalUnrealizedPnl;
        var dailyDd = account.DailyStartingBalance > 0 && dailyPnl < 0
            ? Math.Round((Math.Abs(dailyPnl) / account.DailyStartingBalance) * 100m, 2)
            : 0m;

        var accountDto = new ForexAccountDto(
            account.Id,
            account.Name,
            account.BaseCurrency,
            account.Balance,
            account.Equity,
            account.UsedMargin,
            account.FreeMargin,
            account.MarginLevelPercent,
            account.Leverage,
            account.IsDemo,
            account.PeakEquity,
            account.DailyStartingBalance,
            dailyPnl,
            dailyDd
        );

        var pendingOrderDtos = pendingOrdersEntities.Select(o => new ForexOrderDto(
            o.Id, o.AccountId, o.Symbol, o.Direction, o.OrderType, o.LotSize, o.RequestedPrice,
            o.FillPrice, o.StopLoss, o.TakeProfit, o.SlippagePips, o.Status, o.IdempotencyKey,
            o.RejectionReason, o.CreatedOn, o.FilledAtUtc
        )).ToList();

        var recentClosedDtos = recentClosedEntities.OrderByDescending(t => t.CloseTimeUtc).Take(15).Select(t => new ForexTradeDto(
            t.Id, t.AccountId, t.OrderId, t.Symbol, t.Direction, t.LotSize, t.OpenPrice,
            t.ClosePrice, t.StopLoss, t.TakeProfit, t.TrailingStopPips, t.BreakEvenTriggered,
            t.MarginRequired, t.SwapCost, t.Commission, t.GrossProfit, t.NetProfit, t.Pips,
            t.Status, t.ExitReason, t.OpenTimeUtc, t.CloseTimeUtc, t.Notes, t.ScreenshotUrl,
            t.ClosePrice ?? t.OpenPrice, t.NetProfit, t.Pips
        )).ToList();

        return Result<TradeTerminalDashboardDto>.Success(new TradeTerminalDashboardDto(
            accountDto,
            openTradeDtos,
            pendingOrderDtos,
            recentClosedDtos,
            overview.Quotes
        ));
    }
}
