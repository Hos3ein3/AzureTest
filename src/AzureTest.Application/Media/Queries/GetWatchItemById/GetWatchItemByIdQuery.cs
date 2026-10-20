using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Media.Queries.GetWatchList;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media.Queries.GetWatchItemById;

public sealed record GetWatchItemByIdQuery(Guid UserId, Guid WatchItemId) : IQuery<Result<WatchItemDto>>;

public sealed class GetWatchItemByIdQueryHandler : IQueryHandler<GetWatchItemByIdQuery, Result<WatchItemDto>>
{
    private readonly INoSqlUnitOfWork _uow;

    public GetWatchItemByIdQueryHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<WatchItemDto>> HandleAsync(GetWatchItemByIdQuery request, CancellationToken ct = default)
    {
        var item = await _uow.WatchItems.GetByIdAsync(request.WatchItemId, ct);
        if (item == null)
        {
            return Result.Failure<WatchItemDto>("Watch item not found.");
        }

        if (item.UserId != request.UserId)
        {
            return Result.Failure<WatchItemDto>("Not authorized to view this watch item.");
        }

        return Result.Success(GetWatchListQuery.Mapping.ToDto(item));
    }
}
