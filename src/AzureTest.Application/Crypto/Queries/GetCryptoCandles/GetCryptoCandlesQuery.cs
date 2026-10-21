using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoCandles;

public sealed record GetCryptoCandlesQuery(
    string Symbol,
    CryptoTimeframe Timeframe,
    int Limit = 120) : IQuery<Result<IReadOnlyList<CryptoCandleDto>>>;

public sealed class GetCryptoCandlesQueryValidator : AbstractValidator<GetCryptoCandlesQuery>
{
    public GetCryptoCandlesQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
        RuleFor(x => x.Limit).InclusiveBetween(10, 500);
    }
}

public sealed class GetCryptoCandlesQueryHandler : IQueryHandler<GetCryptoCandlesQuery, Result<IReadOnlyList<CryptoCandleDto>>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoCandlesQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<IReadOnlyList<CryptoCandleDto>>> HandleAsync(GetCryptoCandlesQuery query, CancellationToken ct)
    {
        var candles = await _marketData.GetCandlesAsync(query.Symbol, query.Timeframe, query.Limit, ct);
        return Result<IReadOnlyList<CryptoCandleDto>>.Success(candles);
    }
}
