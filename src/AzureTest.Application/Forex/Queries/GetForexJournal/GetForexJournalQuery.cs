using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexJournal;

public sealed record GetForexJournalQuery(ForexJournalFilter Filter)
    : IQuery<Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>>;

public sealed class GetForexJournalQueryValidator : AbstractValidator<GetForexJournalQuery>
{
    public GetForexJournalQueryValidator()
    {
        RuleFor(x => x.Filter.Page).GreaterThan(0);
        RuleFor(x => x.Filter.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetForexJournalQueryHandler
    : IQueryHandler<GetForexJournalQuery, Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetForexJournalQueryHandler(IUnitOfWork unitOfWork, ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>> HandleAsync(
        GetForexJournalQuery query,
        CancellationToken cancellationToken)
    {
        var entries = await _unitOfWork.ForexJournalEntries.FindAsync(j => j.UserId == _currentUser.Id);
        var filteredEntries = entries.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query.Filter.Symbol))
        {
            filteredEntries = filteredEntries.Where(e => string.Equals(e.Symbol, query.Filter.Symbol, StringComparison.OrdinalIgnoreCase));
        }
        if (!string.IsNullOrWhiteSpace(query.Filter.SetupType))
        {
            filteredEntries = filteredEntries.Where(e => e.SetupType.Contains(query.Filter.SetupType, StringComparison.OrdinalIgnoreCase));
        }

        var entryDtos = filteredEntries
            .OrderByDescending(e => e.TradeDateUtc)
            .Select(e => new ForexJournalEntryDto(
                e.Id, e.TradeId, e.Symbol, e.TradeDateUtc, e.MarketSession, e.SetupType,
                e.Emotions, e.Mistakes, e.LessonsLearned, e.DetailedNotes, e.ScreenshotUrl,
                e.ExecutionRating, e.FollowedPlan, e.CreatedOn
            )).ToList();

        // Calculate tax report summary for the current year
        var currentYear = DateTime.UtcNow.Year;
        var closedTrades = await _unitOfWork.ForexTrades.FindAsync(
            t => t.UserId == _currentUser.Id && t.Status == ForexTradeStatus.Closed && t.CloseTimeUtc.HasValue && t.CloseTimeUtc.Value.Year == currentYear);

        var taxRows = closedTrades.Select(t => new TaxTradeRowDto(
            t.OpenTimeUtc,
            t.CloseTimeUtc!.Value,
            t.Symbol,
            t.Direction,
            t.LotSize,
            t.OpenPrice,
            t.ClosePrice ?? t.OpenPrice,
            t.NetProfit,
            t.Commission + t.SwapCost
        )).ToList();

        decimal grossProfits = closedTrades.Where(t => t.NetProfit > 0).Sum(t => t.NetProfit);
        decimal losses = closedTrades.Where(t => t.NetProfit < 0).Sum(t => Math.Abs(t.NetProfit));
        decimal netRealized = closedTrades.Sum(t => t.NetProfit);
        decimal fees = closedTrades.Sum(t => t.Commission + t.SwapCost);

        var taxSummary = new TaxReportSummaryDto(currentYear, grossProfits, losses, netRealized, fees, taxRows.Count, taxRows);

        return Result<(IReadOnlyList<ForexJournalEntryDto> Entries, TaxReportSummaryDto TaxSummary)>.Success((entryDtos, taxSummary));
    }
}
