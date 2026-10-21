using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoAlerts;

public sealed record GetCryptoAlertsQuery(Guid UserId) : IQuery<Result<IReadOnlyList<CryptoAlertDto>>>;

public sealed class GetCryptoAlertsQueryValidator : AbstractValidator<GetCryptoAlertsQuery>
{
    public GetCryptoAlertsQueryValidator()
    {
    }
}

public sealed class GetCryptoAlertsQueryHandler : IQueryHandler<GetCryptoAlertsQuery, Result<IReadOnlyList<CryptoAlertDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetCryptoAlertsQueryHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CryptoAlertDto>>> HandleAsync(GetCryptoAlertsQuery query, CancellationToken ct)
    {
        var alerts = (await _uow.CryptoAlerts.FindAsync(a => a.UserId == query.UserId))
            .OrderByDescending(a => a.CreatedOn)
            .Select(a => new CryptoAlertDto(
                a.Id,
                a.Symbol,
                a.AlertType,
                a.TargetValue,
                a.Message,
                a.Channel,
                a.IsActive,
                a.IsTriggered,
                a.TriggeredAtUtc,
                a.CreatedOn))
            .ToList();

        return Result<IReadOnlyList<CryptoAlertDto>>.Success(alerts);
    }
}
