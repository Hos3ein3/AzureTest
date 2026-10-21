using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoDexScreener;

public sealed record GetCryptoDexScreenerQuery : IQuery<Result<CryptoDexScreenerHubDto>>;

public sealed class GetCryptoDexScreenerQueryValidator : AbstractValidator<GetCryptoDexScreenerQuery>
{
    public GetCryptoDexScreenerQueryValidator()
    {
    }
}

public sealed class GetCryptoDexScreenerQueryHandler : IQueryHandler<GetCryptoDexScreenerQuery, Result<CryptoDexScreenerHubDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoDexScreenerQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoDexScreenerHubDto>> HandleAsync(GetCryptoDexScreenerQuery query, CancellationToken ct)
    {
        var hub = await _marketData.GetDexScreenerAsync(ct);
        return Result<CryptoDexScreenerHubDto>.Success(hub);
    }
}
