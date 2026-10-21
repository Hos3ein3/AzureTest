using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoOrderBook;

public sealed record GetCryptoOrderBookQuery(string Symbol, int Limit = 20) : IQuery<Result<CryptoOrderBookDto>>;

public sealed class GetCryptoOrderBookQueryValidator : AbstractValidator<GetCryptoOrderBookQuery>
{
    public GetCryptoOrderBookQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
        RuleFor(x => x.Limit).InclusiveBetween(5, 50);
    }
}

public sealed class GetCryptoOrderBookQueryHandler : IQueryHandler<GetCryptoOrderBookQuery, Result<CryptoOrderBookDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoOrderBookQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoOrderBookDto>> HandleAsync(GetCryptoOrderBookQuery query, CancellationToken ct)
    {
        var book = await _marketData.GetOrderBookAsync(query.Symbol, query.Limit, ct);
        return Result<CryptoOrderBookDto>.Success(book);
    }
}
