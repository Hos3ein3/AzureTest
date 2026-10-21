using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoArbitrageHub;

public sealed record GetCryptoArbitrageHubQuery : IQuery<Result<CryptoArbitrageHubDto>>;

public sealed class GetCryptoArbitrageHubQueryValidator : AbstractValidator<GetCryptoArbitrageHubQuery>
{
    public GetCryptoArbitrageHubQueryValidator()
    {
    }
}

public sealed class GetCryptoArbitrageHubQueryHandler : IQueryHandler<GetCryptoArbitrageHubQuery, Result<CryptoArbitrageHubDto>>
{
    private readonly ICryptoArbitrageService _arbitrageService;

    public GetCryptoArbitrageHubQueryHandler(ICryptoArbitrageService arbitrageService)
    {
        _arbitrageService = arbitrageService;
    }

    public async Task<Result<CryptoArbitrageHubDto>> HandleAsync(GetCryptoArbitrageHubQuery query, CancellationToken ct)
    {
        return await _arbitrageService.GetArbitrageHubAsync(ct);
    }
}
