using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoMarketStructure;

public sealed record GetCryptoMarketStructureQuery(string Symbol, CryptoTimeframe Timeframe) : IQuery<Result<CryptoMarketStructureDto>>;

public sealed class GetCryptoMarketStructureQueryValidator : AbstractValidator<GetCryptoMarketStructureQuery>
{
    public GetCryptoMarketStructureQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetCryptoMarketStructureQueryHandler : IQueryHandler<GetCryptoMarketStructureQuery, Result<CryptoMarketStructureDto>>
{
    private readonly ICryptoAnalysisService _analysisService;

    public GetCryptoMarketStructureQueryHandler(ICryptoAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    public async Task<Result<CryptoMarketStructureDto>> HandleAsync(GetCryptoMarketStructureQuery query, CancellationToken ct)
    {
        var structure = await _analysisService.GetMarketStructureAsync(query.Symbol, query.Timeframe, ct);
        return Result<CryptoMarketStructureDto>.Success(structure);
    }
}
