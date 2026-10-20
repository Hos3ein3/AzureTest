using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Interfaces.Data.NoSql;
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Documents.Media;

namespace AzureTest.Application.Media.Queries.GetWatchList;

public sealed record GetWatchListQuery(Guid UserId, WatchListFilter Filter) : IQuery<Result<PagedResult<WatchItemDto, WatchListFilter>>>
{
    public static class Mapping
    {
        public static WatchItemDto ToDto(WatchItemDocument item) =>
            new(
                item.Id,
                item.UserId,
                item.TmdbId,
                item.Type,
                item.Title,
                item.OriginalTitle,
                item.PosterPath,
                item.BackdropPath,
                item.Overview,
                item.ReleaseDate,
                item.TmdbVoteAverage,
                item.Status,
                item.CurrentSeason,
                item.CurrentEpisode,
                item.TotalSeasons,
                item.TotalEpisodes,
                item.RuntimeMinutes,
                item.UserRating,
                item.UserReview,
                item.WatchedDate,
                item.LastWatchedAt,
                item.Genres,
                item.GetNextEpisodeLabel(),
                item.GetEstimatedWatchedMinutes()
            );
    }
}

public sealed class GetWatchListQueryHandler : IQueryHandler<GetWatchListQuery, Result<PagedResult<WatchItemDto, WatchListFilter>>>
{
    private readonly INoSqlUnitOfWork _uow;

    public GetWatchListQueryHandler(INoSqlUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<PagedResult<WatchItemDto, WatchListFilter>>> HandleAsync(GetWatchListQuery request, CancellationToken ct = default)
    {
        var allItems = await _uow.WatchItems.GetUserWatchListAsync(request.UserId, request.Filter.Status, request.Filter.Type, ct);

        IEnumerable<WatchItemDocument> query = allItems;

        if (!string.IsNullOrWhiteSpace(request.Filter.SearchTerm))
        {
            var term = request.Filter.SearchTerm.Trim().ToLowerInvariant();
            query = query.Where(x => x.Title.ToLowerInvariant().Contains(term) ||
                                     (x.OriginalTitle != null && x.OriginalTitle.ToLowerInvariant().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(request.Filter.Genre))
        {
            query = query.Where(x => x.Genres.Any(g => string.Equals(g, request.Filter.Genre, StringComparison.OrdinalIgnoreCase)));
        }

        var filteredList = query.ToList();
        var totalCount = filteredList.Count;
        var pageNumber = Math.Max(1, request.Filter.PageNumber);
        var pageSize = Math.Max(1, request.Filter.PageSize);

        var pagedItems = filteredList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(GetWatchListQuery.Mapping.ToDto)
            .ToList();

        var pagedResult = new PagedResult<WatchItemDto, WatchListFilter>(
            pagedItems,
            totalCount,
            pageNumber,
            pageSize,
            request.Filter
        );

        return Result.Success(pagedResult);
    }
}
