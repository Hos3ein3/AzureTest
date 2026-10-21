using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Forex.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Forex;
using AzureTest.Domain.Entities.Forex.Enums;
using FluentValidation;

namespace AzureTest.Application.Forex.Queries.GetForexAnalysis;

public sealed record GetForexAnalysisQuery(string Symbol = "EUR/USD", ForexTimeframe Timeframe = ForexTimeframe.H1)
    : IQuery<Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>>;

public sealed class GetForexAnalysisQueryValidator : AbstractValidator<GetForexAnalysisQuery>
{
    public GetForexAnalysisQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetForexAnalysisQueryHandler
    : IQueryHandler<GetForexAnalysisQuery, Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>>
{
    private readonly IForexAnalysisService _analysisService;

    public GetForexAnalysisQueryHandler(IForexAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    public async Task<Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>> HandleAsync(
        GetForexAnalysisQuery query,
        CancellationToken cancellationToken)
    {
        var tech = await _analysisService.GetTechnicalOverviewAsync(query.Symbol, query.Timeframe, cancellationToken);
        var fund = await _analysisService.GetFundamentalOverviewAsync(cancellationToken);

        return Result<(TechnicalOverviewDto Technical, FundamentalOverviewDto Fundamental)>.Success((tech, fund));
    }
}
