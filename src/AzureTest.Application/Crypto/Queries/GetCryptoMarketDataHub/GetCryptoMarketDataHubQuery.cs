using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoMarketDataHub;

public sealed record GetCryptoMarketDataHubQuery(string Symbol = "BTCUSDT", CryptoTimeframe Timeframe = CryptoTimeframe.H1) 
    : IQuery<Result<CryptoMarketDataHubDto>>;

public sealed class GetCryptoMarketDataHubQueryValidator : AbstractValidator<GetCryptoMarketDataHubQuery>
{
    public GetCryptoMarketDataHubQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetCryptoMarketDataHubQueryHandler : IQueryHandler<GetCryptoMarketDataHubQuery, Result<CryptoMarketDataHubDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoMarketDataHubQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoMarketDataHubDto>> HandleAsync(GetCryptoMarketDataHubQuery query, CancellationToken ct)
    {
        var hub = await _marketData.GetMarketDataHubAsync(query.Symbol, query.Timeframe, ct);
        return Result<CryptoMarketDataHubDto>.Success(hub);
    }
}
