using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoScreener;

public sealed record GetCryptoScreenerQuery : IQuery<Result<IReadOnlyList<CryptoScreenerItemDto>>>;

public sealed class GetCryptoScreenerQueryValidator : AbstractValidator<GetCryptoScreenerQuery>
{
    public GetCryptoScreenerQueryValidator()
    {
    }
}

public sealed class GetCryptoScreenerQueryHandler : IQueryHandler<GetCryptoScreenerQuery, Result<IReadOnlyList<CryptoScreenerItemDto>>>
{
    private readonly ICryptoAnalysisService _analysisService;

    public GetCryptoScreenerQueryHandler(ICryptoAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    public async Task<Result<IReadOnlyList<CryptoScreenerItemDto>>> HandleAsync(GetCryptoScreenerQuery query, CancellationToken ct)
    {
        var screener = await _analysisService.RunScreenerAsync(ct);
        return Result<IReadOnlyList<CryptoScreenerItemDto>>.Success(screener);
    }
}
