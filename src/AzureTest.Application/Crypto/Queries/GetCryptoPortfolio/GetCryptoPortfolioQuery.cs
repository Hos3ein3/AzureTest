using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoPortfolio;

public sealed record GetCryptoPortfolioQuery(Guid UserId) : IQuery<Result<(CryptoAccountDto Account, IReadOnlyList<CryptoPositionDto> OpenPositions, IReadOnlyList<CryptoPositionDto> ClosedTrades, IReadOnlyList<CryptoJournalEntryDto> JournalEntries)>>;

public sealed class GetCryptoPortfolioQueryValidator : AbstractValidator<GetCryptoPortfolioQuery>
{
    public GetCryptoPortfolioQueryValidator()
    {
    }
}

public sealed class GetCryptoPortfolioQueryHandler : IQueryHandler<GetCryptoPortfolioQuery, Result<(CryptoAccountDto Account, IReadOnlyList<CryptoPositionDto> OpenPositions, IReadOnlyList<CryptoPositionDto> ClosedTrades, IReadOnlyList<CryptoJournalEntryDto> JournalEntries)>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoPortfolioQueryHandler(IUnitOfWork uow, ICryptoMarketDataService marketData)
    {
        _uow = uow;
        _marketData = marketData;
    }

    public async Task<Result<(CryptoAccountDto Account, IReadOnlyList<CryptoPositionDto> OpenPositions, IReadOnlyList<CryptoPositionDto> ClosedTrades, IReadOnlyList<CryptoJournalEntryDto> JournalEntries)>> HandleAsync(GetCryptoPortfolioQuery query, CancellationToken ct)
    {
        var accounts = await _uow.CryptoAccounts.FindAsync(a => a.UserId == query.UserId);
        var account = accounts.FirstOrDefault() ?? CryptoAccount.Create(query.UserId, Guid.Empty, "Primary Trading Account", "USDT", CryptoTradingMode.PaperTrading);

        var allPositions = (await _uow.CryptoPositions.FindAsync(p => p.AccountId == account.Id)).ToList();
        var openPositions = allPositions.Where(p => p.Status == CryptoPositionStatus.Open).ToList();
        var closedPositions = allPositions.Where(p => p.Status != CryptoPositionStatus.Open).OrderByDescending(p => p.ClosedAtUtc).ToList();

        decimal totalUnrealizedPnl = 0m;
        foreach (var pos in openPositions)
        {
            decimal curPrice = await _marketData.GetCurrentPriceAsync(pos.Symbol, ct);
            if (curPrice > 0m) pos.UpdatePrice(curPrice);
            totalUnrealizedPnl += pos.UnrealizedPnl;
        }

        account.UpdateBalance(0m, openPositions.Sum(p => p.InitialMargin), totalUnrealizedPnl);
        await _uow.SaveChangesAsync(ct);

        var openDtos = openPositions.Select(p => new CryptoPositionDto(
            p.Id, p.AccountId, p.Symbol, p.Side, p.Quantity, p.EntryPrice, p.CurrentPrice, p.ClosePrice, p.Leverage,
            p.InitialMargin, p.LiquidationPrice, p.UnrealizedPnl, p.RealizedPnl,
            p.InitialMargin > 0 ? Math.Round((p.UnrealizedPnl / p.InitialMargin) * 100m, 2) : 0m,
            p.StopLoss, p.TakeProfit, p.TrailingStopDistance, p.Status, p.OpenedAtUtc, p.ClosedAtUtc)).ToList();

        var closedDtos = closedPositions.Select(p => new CryptoPositionDto(
            p.Id, p.AccountId, p.Symbol, p.Side, p.Quantity, p.EntryPrice, p.CurrentPrice, p.ClosePrice, p.Leverage,
            p.InitialMargin, p.LiquidationPrice, p.UnrealizedPnl, p.RealizedPnl,
            p.InitialMargin > 0 ? Math.Round((p.RealizedPnl / p.InitialMargin) * 100m, 2) : 0m,
            p.StopLoss, p.TakeProfit, p.TrailingStopDistance, p.Status, p.OpenedAtUtc, p.ClosedAtUtc)).ToList();

        var journalEntries = (await _uow.CryptoJournalEntries.FindAsync(j => j.UserId == query.UserId))
            .OrderByDescending(j => j.CreatedOn)
            .Select(j => new CryptoJournalEntryDto(
                j.Id, j.PositionId, j.Symbol, j.StrategyTag, j.EmotionTag, j.Rating, j.Notes, j.LessonsLearned, j.CreatedOn))
            .ToList();

        var accountDto = new CryptoAccountDto(
            account.Id, account.Name, account.BaseCurrency, account.TradingMode, account.CashBalance, account.Equity,
            account.MarginUsed, account.FreeMargin, account.DailyStartingEquity,
            new CurrentDailyLossDto(
                account.CurrentDailyLoss,
                account.DailyStartingEquity > 0 ? Math.Round((account.CurrentDailyLoss / account.DailyStartingEquity) * 100m, 2) : 0m,
                account.DailyLossLimitPercent,
                account.CurrentDailyLoss >= (account.DailyStartingEquity * (account.DailyLossLimitPercent / 100m))),
            account.RiskPercentagePerTrade, account.MaxOpenPositions, account.IsKillSwitchActive, account.KillSwitchReason);

        return Result<(CryptoAccountDto, IReadOnlyList<CryptoPositionDto>, IReadOnlyList<CryptoPositionDto>, IReadOnlyList<CryptoJournalEntryDto>)>.Success(
            (accountDto, openDtos, closedDtos, journalEntries));
    }
}
