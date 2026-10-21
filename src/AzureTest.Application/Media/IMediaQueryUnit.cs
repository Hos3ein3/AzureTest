using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Abstractions;
using AzureTest.Application.Media.Queries.ExportMediaData;
using AzureTest.Application.Media.Queries.GetCustomListById;
using AzureTest.Application.Media.Queries.GetCustomLists;
using AzureTest.Application.Media.Queries.GetDiscoverMedia;
using AzureTest.Application.Media.Queries.GetFollowedPeople;
using AzureTest.Application.Media.Queries.GetMediaCalendar;
using AzureTest.Application.Media.Queries.GetMediaDetails;
using AzureTest.Application.Media.Queries.GetMediaStats;
using AzureTest.Application.Media.Queries.GetMovieNews;
using AzureTest.Application.Media.Queries.GetPersonDetails;
using AzureTest.Application.Media.Queries.GetWatchItemById;
using AzureTest.Application.Media.Queries.GetWatchList;
using AzureTest.Application.Media.Queries.SearchMedia;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media;

public interface IMediaQueryUnit
{
    Task<Result<PagedResult<WatchItemDto, WatchListFilter>>> GetWatchListAsync(GetWatchListQuery query, CancellationToken ct = default);
    Task<Result<WatchItemDto>> GetWatchItemByIdAsync(GetWatchItemByIdQuery query, CancellationToken ct = default);
    Task<Result<TmdbDiscoverDto>> GetDiscoverMediaAsync(GetDiscoverMediaQuery query, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TmdbSearchResultDto>>> SearchMediaAsync(SearchMediaQuery query, CancellationToken ct = default);
    Task<Result<TmdbTitleDetailsDto>> GetMediaDetailsAsync(GetMediaDetailsQuery query, CancellationToken ct = default);
    Task<Result<IReadOnlyList<FollowedPersonDto>>> GetFollowedPeopleAsync(GetFollowedPeopleQuery query, CancellationToken ct = default);
    Task<Result<FollowedPersonDto>> GetPersonDetailsAsync(GetPersonDetailsQuery query, CancellationToken ct = default);
    Task<Result<MediaCalendarDto>> GetMediaCalendarAsync(GetMediaCalendarQuery query, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovieNewsItemDto>>> GetMovieNewsAsync(GetMovieNewsQuery query, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CustomMediaListDto>>> GetCustomListsAsync(GetCustomListsQuery query, CancellationToken ct = default);
    Task<Result<CustomMediaListDto>> GetCustomListByIdAsync(GetCustomListByIdQuery query, CancellationToken ct = default);
    Task<Result<MediaStatsDto>> GetMediaStatsAsync(GetMediaStatsQuery query, CancellationToken ct = default);
    Task<Result<MediaExportDto>> ExportMediaDataAsync(ExportMediaDataQuery query, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TvEpisodeSummaryDto>>> GetSeasonEpisodesAsync(AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery query, CancellationToken ct = default);
    Task<Result<MediaRecommendationsDto>> GetSmartRecommendationsAsync(AzureTest.Application.Media.Queries.GetSmartRecommendations.GetSmartRecommendationsQuery query, CancellationToken ct = default);
}
