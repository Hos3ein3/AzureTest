using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoTradeTerminal;

public sealed record GetCryptoTradeTerminalQuery(Guid UserId, string SelectedSymbol = "BTCUSDT") : IQuery<Result<TradeTerminalDashboardDto>>;

public sealed class GetCryptoTradeTerminalQueryValidator : AbstractValidator<GetCryptoTradeTerminalQuery>
{
    public GetCryptoTradeTerminalQueryValidator()
    {
    }
}

public sealed class GetCryptoTradeTerminalQueryHandler : IQueryHandler<GetCryptoTradeTerminalQuery, Result<TradeTerminalDashboardDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICryptoMarketDataService _marketData;
    private readonly ICryptoSmartExecutionService _smartExecution;
    private readonly ITenantProvider _tenantProvider;

    public GetCryptoTradeTerminalQueryHandler(
        IUnitOfWork uow, 
        ICryptoMarketDataService marketData,
        ICryptoSmartExecutionService smartExecution,
        ITenantProvider tenantProvider)
    {
        _uow = uow;
        _marketData = marketData;
        _smartExecution = smartExecution;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<TradeTerminalDashboardDto>> HandleAsync(GetCryptoTradeTerminalQuery query, CancellationToken ct)
    {
        var isReadOnlyImpersonation = _tenantProvider.IsAdminImpersonating && !_tenantProvider.AllowImpersonationWrites;

        // 1. Get or create primary paper account
        var accounts = await _uow.CryptoAccounts.FindAsync(a => a.UserId == query.UserId);
        var account = accounts.FirstOrDefault();
        if (account == null)
        {
            account = CryptoAccount.Create(query.UserId, _tenantProvider.CurrentTenantId, "Primary Paper Trading", "USDT", CryptoTradingMode.PaperTrading);
            if (!isReadOnlyImpersonation)
            {
                _uow.CryptoAccounts.Add(account);
                await _uow.SaveChangesAsync(ct);
            }
        }

        // 2. Fetch live coin list & current price for selected symbol
        var marketOverview = await _marketData.GetMarketOverviewAsync(new CryptoMarketFilter { PageSize = 15 }, ct);
        decimal currentPrice = await _marketData.GetCurrentPriceAsync(query.SelectedSymbol, ct);

        // 3. Fetch open positions and refresh unrealized PnL with live market price
        var positions = (await _uow.CryptoPositions.FindAsync(p => p.AccountId == account.Id && p.Status == CryptoPositionStatus.Open)).ToList();
        decimal totalUnrealizedPnl = 0m;
        decimal totalMarginUsed = 0m;

        foreach (var pos in positions)
        {
            decimal posMarketPrice = pos.Symbol.Equals(query.SelectedSymbol, StringComparison.OrdinalIgnoreCase)
                ? currentPrice
                : await _marketData.GetCurrentPriceAsync(pos.Symbol, ct);

            pos.UpdatePrice(posMarketPrice);
            totalUnrealizedPnl += pos.UnrealizedPnl;
            totalMarginUsed += pos.InitialMargin;
        }

        account.UpdateBalance(cashDelta: 0m, marginUsed: totalMarginUsed, unrealizedPnl: totalUnrealizedPnl);
        if (!isReadOnlyImpersonation)
        {
            await _uow.SaveChangesAsync(ct);
        }

        var positionDtos = positions.Select(p => new CryptoPositionDto(
            p.Id,
            p.AccountId,
            p.Symbol,
            p.Side,
            p.Quantity,
            p.EntryPrice,
            p.CurrentPrice,
            p.ClosePrice,
            p.Leverage,
            p.InitialMargin,
            p.LiquidationPrice,
            p.UnrealizedPnl,
            p.RealizedPnl,
            p.InitialMargin > 0 ? Math.Round((p.UnrealizedPnl / p.InitialMargin) * 100m, 2) : 0m,
            p.StopLoss,
            p.TakeProfit,
            p.TrailingStopDistance,
            p.Status,
            p.OpenedAtUtc,
            p.ClosedAtUtc)).ToList();

        // 4. Fetch recent orders
        var orders = (await _uow.CryptoOrders.FindAsync(o => o.AccountId == account.Id))
            .OrderByDescending(o => o.CreatedOn)
            .Take(15)
            .Select(o => new CryptoOrderDto(
                o.Id,
                o.AccountId,
                o.Symbol,
                o.Side,
                o.OrderType,
                o.Quantity,
                o.RequestedPrice,
                o.FillPrice,
                o.StopPrice,
                o.Status,
                o.Commission,
                o.Slippage,
                o.IdempotencyKey,
                o.RejectionReason,
                o.CreatedOn,
                o.FilledAtUtc)).ToList();

        // 5. Order book & recent trades
        var depth = await _marketData.GetOrderBookAsync(query.SelectedSymbol, 15, ct);
        var trades = await _marketData.GetRecentTradesAsync(query.SelectedSymbol, 15, ct);
        var futuresData = await _marketData.GetFuturesFundingAsync(query.SelectedSymbol, ct);

        var accountDto = new CryptoAccountDto(
            account.Id,
            account.Name,
            account.BaseCurrency,
            account.TradingMode,
            account.CashBalance,
            account.Equity,
            account.MarginUsed,
            account.FreeMargin,
            account.DailyStartingEquity,
            new CurrentDailyLossDto(
                account.CurrentDailyLoss,
                account.DailyStartingEquity > 0 ? Math.Round((account.CurrentDailyLoss / account.DailyStartingEquity) * 100m, 2) : 0m,
                account.DailyLossLimitPercent,
                account.CurrentDailyLoss >= (account.DailyStartingEquity * (account.DailyLossLimitPercent / 100m))),
            account.RiskPercentagePerTrade,
            account.MaxOpenPositions,
            account.IsKillSwitchActive,
            account.KillSwitchReason);

        // 6. Fetch active smart execution algorithms & trailing stop monitors
        var twapRes = await _smartExecution.GetActiveTwapOrdersAsync(account.Id, ct);
        var icebergRes = await _smartExecution.GetActiveIcebergOrdersAsync(account.Id, ct);
        var trailingList = new System.Collections.Generic.List<CryptoTrailingStopOrderDto>();

        foreach (var pos in positions.Where(p => p.TrailingStopDistance.HasValue && p.TrailingStopDistance > 0))
        {
            var trailRes = await _smartExecution.EvaluateTrailingStopAsync(pos.Id, pos.CurrentPrice, pos.TrailingStopDistance, ct);
            if (trailRes.IsSuccess && trailRes.Value != null)
            {
                trailingList.Add(trailRes.Value);
            }
        }

        var terminalDto = new TradeTerminalDashboardDto(
            Account: accountDto,
            OpenPositions: positionDtos,
            RecentOrders: orders,
            ActiveCoins: marketOverview.Coins,
            SelectedSymbol: query.SelectedSymbol,
            CurrentPrice: currentPrice,
            Depth: depth,
            RecentTrades: trades,
            FuturesData: futuresData,
            ActiveTwapOrders: twapRes.Value ?? System.Array.Empty<CryptoTwapOrderDto>(),
            ActiveIcebergOrders: icebergRes.Value ?? System.Array.Empty<CryptoIcebergOrderDto>(),
            TrailingStops: trailingList);

        return Result<TradeTerminalDashboardDto>.Success(terminalDto);
    }
}
