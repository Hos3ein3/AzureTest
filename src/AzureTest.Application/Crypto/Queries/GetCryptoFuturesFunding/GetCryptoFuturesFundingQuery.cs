using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoFuturesFunding;

public sealed record GetCryptoFuturesFundingQuery(string Symbol) : IQuery<Result<CryptoFuturesFundingDto>>;

public sealed class GetCryptoFuturesFundingQueryValidator : AbstractValidator<GetCryptoFuturesFundingQuery>
{
    public GetCryptoFuturesFundingQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetCryptoFuturesFundingQueryHandler : IQueryHandler<GetCryptoFuturesFundingQuery, Result<CryptoFuturesFundingDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoFuturesFundingQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoFuturesFundingDto>> HandleAsync(GetCryptoFuturesFundingQuery query, CancellationToken ct)
    {
        var funding = await _marketData.GetFuturesFundingAsync(query.Symbol, ct);
        return Result<CryptoFuturesFundingDto>.Success(funding);
    }
}
