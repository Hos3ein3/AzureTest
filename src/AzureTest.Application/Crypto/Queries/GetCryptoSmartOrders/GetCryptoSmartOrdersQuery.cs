using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Crypto.Services;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Crypto;
using FluentValidation;

namespace AzureTest.Application.Crypto.Queries.GetCryptoSmartOrders;

public sealed record GetCryptoSmartOrdersQuery(Guid AccountId) 
    : IQuery<Result<(IReadOnlyList<CryptoTwapOrderDto> TwapOrders, IReadOnlyList<CryptoIcebergOrderDto> IcebergOrders)>>;

public sealed class GetCryptoSmartOrdersQueryValidator : AbstractValidator<GetCryptoSmartOrdersQuery>
{
    public GetCryptoSmartOrdersQueryValidator()
    {
    }
}

public sealed class GetCryptoSmartOrdersQueryHandler 
    : IQueryHandler<GetCryptoSmartOrdersQuery, Result<(IReadOnlyList<CryptoTwapOrderDto> TwapOrders, IReadOnlyList<CryptoIcebergOrderDto> IcebergOrders)>>
{
    private readonly ICryptoSmartExecutionService _smartExecution;

    public GetCryptoSmartOrdersQueryHandler(ICryptoSmartExecutionService smartExecution)
    {
        _smartExecution = smartExecution;
    }

    public async Task<Result<(IReadOnlyList<CryptoTwapOrderDto> TwapOrders, IReadOnlyList<CryptoIcebergOrderDto> IcebergOrders)>> HandleAsync(
        GetCryptoSmartOrdersQuery query, 
        CancellationToken ct)
    {
        var twapRes = await _smartExecution.GetActiveTwapOrdersAsync(query.AccountId, ct);
        var icebergRes = await _smartExecution.GetActiveIcebergOrdersAsync(query.AccountId, ct);

        var twapList = twapRes.Value ?? Array.Empty<CryptoTwapOrderDto>();
        var icebergList = icebergRes.Value ?? Array.Empty<CryptoIcebergOrderDto>();

        return Result<(IReadOnlyList<CryptoTwapOrderDto> TwapOrders, IReadOnlyList<CryptoIcebergOrderDto> IcebergOrders)>
            .Success((twapList, icebergList));
    }
}
