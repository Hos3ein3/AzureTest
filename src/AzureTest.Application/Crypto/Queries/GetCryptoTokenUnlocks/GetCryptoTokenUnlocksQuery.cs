using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoTokenUnlocks;

public sealed record GetCryptoTokenUnlocksQuery : IQuery<Result<CryptoTokenUnlocksHubDto>>;

public sealed class GetCryptoTokenUnlocksQueryValidator : AbstractValidator<GetCryptoTokenUnlocksQuery>
{
    public GetCryptoTokenUnlocksQueryValidator()
    {
    }
}

public sealed class GetCryptoTokenUnlocksQueryHandler : IQueryHandler<GetCryptoTokenUnlocksQuery, Result<CryptoTokenUnlocksHubDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoTokenUnlocksQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoTokenUnlocksHubDto>> HandleAsync(GetCryptoTokenUnlocksQuery query, CancellationToken ct)
    {
        var hub = await _marketData.GetTokenUnlocksAsync(ct);
        return Result<CryptoTokenUnlocksHubDto>.Success(hub);
    }
}
