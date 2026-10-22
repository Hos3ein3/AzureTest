using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using AzureTest.Domain.Entities.Crypto.Enums;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoSettings;

public sealed record GetCryptoSettingsQuery(Guid UserId) : IQuery<Result<IReadOnlyList<CryptoApiSettingsDto>>>;

public sealed class GetCryptoSettingsQueryValidator : AbstractValidator<GetCryptoSettingsQuery>
{
    public GetCryptoSettingsQueryValidator()
    {
    }
}

public sealed class GetCryptoSettingsQueryHandler : IQueryHandler<GetCryptoSettingsQuery, Result<IReadOnlyList<CryptoApiSettingsDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetCryptoSettingsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CryptoApiSettingsDto>>> HandleAsync(GetCryptoSettingsQuery query, CancellationToken ct)
    {
        var settings = (await _uow.CryptoApiSettings.FindAsync(s => s.UserId == query.UserId)).ToList();

        // Ensure default exchanges exist in the returned list
        var defaultExchanges = new[] { CryptoExchange.Binance, CryptoExchange.Kraken, CryptoExchange.Bybit, CryptoExchange.Coinbase, CryptoExchange.Okx, CryptoExchange.CoinGecko };
        var dtos = new List<CryptoApiSettingsDto>();

        foreach (var ex in defaultExchanges)
        {
            var existing = settings.FirstOrDefault(s => s.Exchange == ex);
            if (existing != null)
            {
                dtos.Add(new CryptoApiSettingsDto(
                    existing.Exchange,
                    existing.ApiKeyMasked,
                    existing.IsTestnet,
                    existing.IsActive,
                    existing.IsConnected,
                    existing.LastTestedAtUtc,
                    existing.ConnectionStatusMessage));
            }
            else
            {
                dtos.Add(new CryptoApiSettingsDto(
                    ex,
                    "",
                    true,
                    false,
                    false,
                    null,
                    "Not configured (Public free endpoints currently active)"));
            }
        }

        return Result<IReadOnlyList<CryptoApiSettingsDto>>.Success(dtos);
    }
}
