using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoDerivativesHub;

public sealed record GetCryptoDerivativesHubQuery(string Symbol = "BTCUSDT") 
    : IQuery<Result<CryptoDerivativesHubDto>>;

public sealed class GetCryptoDerivativesHubQueryValidator : AbstractValidator<GetCryptoDerivativesHubQuery>
{
    public GetCryptoDerivativesHubQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetCryptoDerivativesHubQueryHandler : IQueryHandler<GetCryptoDerivativesHubQuery, Result<CryptoDerivativesHubDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoDerivativesHubQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoDerivativesHubDto>> HandleAsync(GetCryptoDerivativesHubQuery query, CancellationToken ct)
    {
        var hub = await _marketData.GetDerivativesHubAsync(query.Symbol, ct);
        return Result<CryptoDerivativesHubDto>.Success(hub);
    }
}
