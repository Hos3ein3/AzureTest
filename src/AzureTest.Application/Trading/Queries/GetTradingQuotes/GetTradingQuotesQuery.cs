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
using AzureTest.Domain.Entities.Trading.Enums;

namespace AzureTest.Application.Trading.Queries.GetTradingQuotes;

public sealed record GetTradingQuotesQuery(
    Guid UserId,
    Guid TenantId,
    AssetClass? AssetClass = null,
    string? Search = null) : IQuery<Result<IReadOnlyList<TradingQuoteDto>>>;

public sealed class GetTradingQuotesQueryHandler : IQueryHandler<GetTradingQuotesQuery, Result<IReadOnlyList<TradingQuoteDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ITradingMarketDataService _marketData;

    public GetTradingQuotesQueryHandler(IUnitOfWork uow, ITradingMarketDataService marketData)
    {
        _uow = uow;
        _marketData = marketData;
    }

    public async Task<Result<IReadOnlyList<TradingQuoteDto>>> HandleAsync(GetTradingQuotesQuery q, CancellationToken ct)
    {
        var watchlists = await _uow.TradingWatchlists.GetAllAsync();
        var watchlisted = new HashSet<string>(
            watchlists.Where(w => w.UserId == q.UserId && (q.TenantId == Guid.Empty || w.TenantId == q.TenantId)).Select(w => w.Symbol),
            StringComparer.OrdinalIgnoreCase);

        var quotes = (await _marketData.GetAllQuotesAsync(watchlisted, ct)).AsEnumerable();

        if (q.AssetClass.HasValue)
        {
            quotes = quotes.Where(x => x.AssetClass == q.AssetClass.Value);
        }

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim().ToLowerInvariant();
            quotes = quotes.Where(x => x.Symbol.ToLowerInvariant().Contains(search) || x.AssetName.ToLowerInvariant().Contains(search));
        }

        return Result<IReadOnlyList<TradingQuoteDto>>.Success(quotes.ToList());
    }
}
