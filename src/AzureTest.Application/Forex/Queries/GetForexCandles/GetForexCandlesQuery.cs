using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexCandles;

public sealed record GetForexCandlesQuery(string Symbol, ForexTimeframe Timeframe, int Count = 100) : IQuery<Result<IReadOnlyList<ForexCandleDto>>>;

public sealed class GetForexCandlesQueryValidator : AbstractValidator<GetForexCandlesQuery>
{
    public GetForexCandlesQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
        RuleFor(x => x.Count).InclusiveBetween(10, 500);
    }
}

public sealed class GetForexCandlesQueryHandler : IQueryHandler<GetForexCandlesQuery, Result<IReadOnlyList<ForexCandleDto>>>
{
    private readonly IForexMarketDataService _marketData;

    public GetForexCandlesQueryHandler(IForexMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<IReadOnlyList<ForexCandleDto>>> HandleAsync(GetForexCandlesQuery query, CancellationToken cancellationToken)
    {
        var candles = await _marketData.GetCandlesAsync(query.Symbol, query.Timeframe, query.Count, cancellationToken);
        return Result<IReadOnlyList<ForexCandleDto>>.Success(candles);
    }
}
