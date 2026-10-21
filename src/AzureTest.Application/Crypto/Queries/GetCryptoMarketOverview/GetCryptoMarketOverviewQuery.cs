using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoMarketOverview;

public sealed record GetCryptoMarketOverviewQuery(CryptoMarketFilter Filter) : IQuery<Result<CryptoMarketOverviewDto>>;

public sealed class GetCryptoMarketOverviewQueryValidator : AbstractValidator<GetCryptoMarketOverviewQuery>
{
    public GetCryptoMarketOverviewQueryValidator()
    {
        RuleFor(x => x.Filter).NotNull();
    }
}

public sealed class GetCryptoMarketOverviewQueryHandler : IQueryHandler<GetCryptoMarketOverviewQuery, Result<CryptoMarketOverviewDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoMarketOverviewQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoMarketOverviewDto>> HandleAsync(GetCryptoMarketOverviewQuery query, CancellationToken ct)
    {
        var overview = await _marketData.GetMarketOverviewAsync(query.Filter, ct);
        return Result<CryptoMarketOverviewDto>.Success(overview);
    }
}
