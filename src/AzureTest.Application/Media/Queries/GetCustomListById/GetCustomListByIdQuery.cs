using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Media.Queries.GetCustomLists;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media.Queries.GetCustomListById;

public sealed record GetCustomListByIdQuery(Guid UserId, Guid ListId) : IQuery<Result<CustomMediaListDto>>;

public sealed class GetCustomListByIdQueryHandler : IQueryHandler<GetCustomListByIdQuery, Result<CustomMediaListDto>>
{
    private readonly INoSqlUnitOfWork _uow;

    public GetCustomListByIdQueryHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<CustomMediaListDto>> HandleAsync(GetCustomListByIdQuery request, CancellationToken ct = default)
    {
        var list = await _uow.CustomMediaLists.GetByIdAsync(request.ListId, ct);
        if (list == null)
        {
            return Result.Failure<CustomMediaListDto>("Custom list not found.");
        }

        if (list.UserId != request.UserId)
        {
            return Result.Failure<CustomMediaListDto>("Not authorized to view this list.");
        }

        return Result.Success(GetCustomListsQuery.Mapping.ToDto(list));
    }
}
