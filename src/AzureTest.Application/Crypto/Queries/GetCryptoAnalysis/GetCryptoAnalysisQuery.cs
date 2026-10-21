using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoAnalysis;

public sealed record GetCryptoAnalysisQuery(string Symbol, CryptoTimeframe Timeframe) : IQuery<Result<CryptoAnalysisDto>>;

public sealed class GetCryptoAnalysisQueryValidator : AbstractValidator<GetCryptoAnalysisQuery>
{
    public GetCryptoAnalysisQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetCryptoAnalysisQueryHandler : IQueryHandler<GetCryptoAnalysisQuery, Result<CryptoAnalysisDto>>
{
    private readonly ICryptoAnalysisService _analysisService;

    public GetCryptoAnalysisQueryHandler(ICryptoAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    public async Task<Result<CryptoAnalysisDto>> HandleAsync(GetCryptoAnalysisQuery query, CancellationToken ct)
    {
        var analysis = await _analysisService.GetComprehensiveAnalysisAsync(query.Symbol, query.Timeframe, ct);
        return Result<CryptoAnalysisDto>.Success(analysis);
    }
}
