using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Media.Queries.GetCustomLists;
using AzureTest.Application.Media.Queries.GetWatchList;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media.Queries.ExportMediaData;

public sealed record ExportMediaDataQuery(Guid UserId) : IQuery<Result<MediaExportDto>>;

public sealed class ExportMediaDataQueryHandler : IQueryHandler<ExportMediaDataQuery, Result<MediaExportDto>>
{
    private readonly INoSqlUnitOfWork _uow;

    public ExportMediaDataQueryHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<MediaExportDto>> HandleAsync(ExportMediaDataQuery request, CancellationToken ct = default)
    {
        var items = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, ct: ct);
        var people = await _uow.FollowedPeople.GetUserFollowedPeopleAsync(request.UserId, ct);
        var lists = await _uow.CustomMediaLists.GetUserListsAsync(request.UserId, ct);

        var itemDtos = items.Select(GetWatchListQuery.Mapping.ToDto).ToList();
        var peopleDtos = people.Select(p => new FollowedPersonDto(
            p.Id,
            p.TmdbPersonId,
            p.Name,
            p.KnownForDepartment,
            p.ProfilePath,
            null,
            null,
            null,
            p.FollowedAt,
            p.Notes,
            0,
            0,
            0,
            new(),
            new()
        )).ToList();
        var listDtos = lists.Select(GetCustomListsQuery.Mapping.ToDto).ToList();

        var export = new MediaExportDto(
            "1.0.0",
            DateTime.UtcNow,
            itemDtos,
            peopleDtos,
            listDtos
        );

        return Result.Success(export);
    }
}
