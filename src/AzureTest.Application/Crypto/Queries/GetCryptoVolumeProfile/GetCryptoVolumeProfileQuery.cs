using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoVolumeProfile;

public sealed record GetCryptoVolumeProfileQuery(string Symbol, CryptoTimeframe Timeframe) : IQuery<Result<CryptoVolumeProfileHubDto>>;

public sealed class GetCryptoVolumeProfileQueryValidator : AbstractValidator<GetCryptoVolumeProfileQuery>
{
    public GetCryptoVolumeProfileQueryValidator()
    {
        RuleFor(x => x.Symbol).NotEmpty();
    }
}

public sealed class GetCryptoVolumeProfileQueryHandler : IQueryHandler<GetCryptoVolumeProfileQuery, Result<CryptoVolumeProfileHubDto>>
{
    private readonly ICryptoAnalysisService _analysisService;

    public GetCryptoVolumeProfileQueryHandler(ICryptoAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    public async Task<Result<CryptoVolumeProfileHubDto>> HandleAsync(GetCryptoVolumeProfileQuery query, CancellationToken ct)
    {
        var vp = await _analysisService.GetVolumeProfileAsync(query.Symbol, query.Timeframe, ct);
        return Result<CryptoVolumeProfileHubDto>.Success(vp);
    }
}
