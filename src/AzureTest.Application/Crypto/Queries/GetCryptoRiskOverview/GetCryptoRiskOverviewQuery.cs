using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoRiskOverview;

public sealed record GetCryptoRiskOverviewQuery(Guid UserId) : IQuery<Result<CryptoRiskOverviewDto>>;

public sealed class GetCryptoRiskOverviewQueryValidator : AbstractValidator<GetCryptoRiskOverviewQuery>
{
    public GetCryptoRiskOverviewQueryValidator()
    {
    }
}

public sealed class GetCryptoRiskOverviewQueryHandler : IQueryHandler<GetCryptoRiskOverviewQuery, Result<CryptoRiskOverviewDto>>
{
    private readonly ICryptoRiskService _riskService;

    public GetCryptoRiskOverviewQueryHandler(ICryptoRiskService riskService)
    {
        _riskService = riskService;
    }

    public async Task<Result<CryptoRiskOverviewDto>> HandleAsync(GetCryptoRiskOverviewQuery query, CancellationToken ct)
    {
        var overview = await _riskService.GetRiskOverviewAsync(query.UserId, ct);
        return Result<CryptoRiskOverviewDto>.Success(overview);
    }
}
