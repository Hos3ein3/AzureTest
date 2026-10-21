using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoMarketRegime;

public sealed record GetCryptoMarketRegimeQuery(string Symbol, CryptoTimeframe Timeframe) : IQuery<Result<CryptoMarketRegimeDto>>;

public sealed class GetCryptoMarketRegimeQueryValidator : AbstractValidator<GetCryptoMarketRegimeQuery>
{
    public GetCryptoMarketRegimeQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetCryptoMarketRegimeQueryHandler : IQueryHandler<GetCryptoMarketRegimeQuery, Result<CryptoMarketRegimeDto>>
{
    private readonly ICryptoAnalysisService _analysisService;

    public GetCryptoMarketRegimeQueryHandler(ICryptoAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    public async Task<Result<CryptoMarketRegimeDto>> HandleAsync(GetCryptoMarketRegimeQuery query, CancellationToken ct)
    {
        var regime = await _analysisService.GetMarketRegimeAsync(query.Symbol, query.Timeframe, ct);
        return Result<CryptoMarketRegimeDto>.Success(regime);
    }
}
