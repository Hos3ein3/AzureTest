using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Documents.Media;

namespace AzureTest.Application.Media.Queries.GetCustomLists;

public sealed record GetCustomListsQuery(Guid UserId) : IQuery<Result<IReadOnlyList<CustomMediaListDto>>>
{
    public static class Mapping
    {
        public static CustomMediaListDto ToDto(CustomMediaListDocument list) =>
            new(
                list.Id,
                list.UserId,
                list.Title,
                list.Description,
                list.Items.Select(i => new CustomMediaListItemDto(i.TmdbId, i.Type, i.Title, i.PosterPath, i.Year, i.AddedAt)).ToList(),
                list.CreatedAt,
                list.UpdatedAt
            );
    }
}

public sealed class GetCustomListsQueryHandler : IQueryHandler<GetCustomListsQuery, Result<IReadOnlyList<CustomMediaListDto>>>
{
    private readonly INoSqlUnitOfWork _uow;

    public GetCustomListsQueryHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CustomMediaListDto>>> HandleAsync(GetCustomListsQuery request, CancellationToken ct = default)
    {
        var lists = await _uow.CustomMediaLists.GetUserListsAsync(request.UserId, ct);
        var dtos = lists.Select(GetCustomListsQuery.Mapping.ToDto).ToList();
        return Result.Success<IReadOnlyList<CustomMediaListDto>>(dtos);
    }
}
