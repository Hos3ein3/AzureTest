using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoOnChainHub;

public sealed record GetCryptoOnChainHubQuery : IQuery<Result<CryptoOnChainHubDto>>;

public sealed class GetCryptoOnChainHubQueryValidator : AbstractValidator<GetCryptoOnChainHubQuery>
{
    public GetCryptoOnChainHubQueryValidator()
    {
    }
}

public sealed class GetCryptoOnChainHubQueryHandler : IQueryHandler<GetCryptoOnChainHubQuery, Result<CryptoOnChainHubDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoOnChainHubQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoOnChainHubDto>> HandleAsync(GetCryptoOnChainHubQuery query, CancellationToken ct)
    {
        var hub = await _marketData.GetOnChainHubAsync(ct);
        return Result<CryptoOnChainHubDto>.Success(hub);
    }
}
