using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoExchangeFlow;

public sealed record GetCryptoExchangeFlowQuery : IQuery<Result<CryptoExchangeFlowAnalysisHubDto>>;

public sealed class GetCryptoExchangeFlowQueryValidator : AbstractValidator<GetCryptoExchangeFlowQuery>
{
    public GetCryptoExchangeFlowQueryValidator()
    {
    }
}

public sealed class GetCryptoExchangeFlowQueryHandler : IQueryHandler<GetCryptoExchangeFlowQuery, Result<CryptoExchangeFlowAnalysisHubDto>>
{
    private readonly ICryptoMarketDataService _marketData;

    public GetCryptoExchangeFlowQueryHandler(ICryptoMarketDataService marketData)
    {
        _marketData = marketData;
    }

    public async Task<Result<CryptoExchangeFlowAnalysisHubDto>> HandleAsync(GetCryptoExchangeFlowQuery query, CancellationToken ct)
    {
        var hub = await _marketData.GetExchangeFlowAnalysisAsync(ct);
        return Result<CryptoExchangeFlowAnalysisHubDto>.Success(hub);
    }
}
