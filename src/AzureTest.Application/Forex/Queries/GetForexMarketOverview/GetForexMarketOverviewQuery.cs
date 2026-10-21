using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexMarketOverview;

public sealed record GetForexMarketOverviewQuery(ForexMarketFilter Filter) : IQuery<Result<MarketOverviewDto>>;

public sealed class GetForexMarketOverviewQueryValidator : AbstractValidator<GetForexMarketOverviewQuery>
{
    public GetForexMarketOverviewQueryValidator()
    {
        RuleFor(x => x.Filter.Page).GreaterThan(0);
        RuleFor(x => x.Filter.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetForexMarketOverviewQueryHandler : IQueryHandler<GetForexMarketOverviewQuery, Result<MarketOverviewDto>>
{
    private readonly IForexMarketDataService _marketDataService;

    public GetForexMarketOverviewQueryHandler(IForexMarketDataService marketDataService)
    {
        _marketDataService = marketDataService;
    }

    public async Task<Result<MarketOverviewDto>> HandleAsync(GetForexMarketOverviewQuery query, CancellationToken cancellationToken)
    {
        var overview = await _marketDataService.GetMarketOverviewAsync(query.Filter, cancellationToken);
        return Result<MarketOverviewDto>.Success(overview);
    }
}
