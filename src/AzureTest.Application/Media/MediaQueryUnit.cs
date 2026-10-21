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
using AzureTest.Application.Messaging;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Contracts.DTOs.Media;

namespace AzureTest.Application.Media;

public sealed class MediaQueryUnit : IMediaQueryUnit
{
    private readonly IQueryHandler<GetWatchListQuery, Result<PagedResult<WatchItemDto, WatchListFilter>>> _getWatchList;
    private readonly IQueryHandler<GetWatchItemByIdQuery, Result<WatchItemDto>> _getWatchItemById;
    private readonly IQueryHandler<GetDiscoverMediaQuery, Result<TmdbDiscoverDto>> _getDiscover;
    private readonly IQueryHandler<SearchMediaQuery, Result<IReadOnlyList<TmdbSearchResultDto>>> _searchMedia;
    private readonly IQueryHandler<GetMediaDetailsQuery, Result<TmdbTitleDetailsDto>> _getMediaDetails;
    private readonly IQueryHandler<GetFollowedPeopleQuery, Result<IReadOnlyList<FollowedPersonDto>>> _getFollowedPeople;
    private readonly IQueryHandler<GetPersonDetailsQuery, Result<FollowedPersonDto>> _getPersonDetails;
    private readonly IQueryHandler<GetMediaCalendarQuery, Result<MediaCalendarDto>> _getCalendar;
    private readonly IQueryHandler<GetMovieNewsQuery, Result<IReadOnlyList<MovieNewsItemDto>>> _getNews;
    private readonly IQueryHandler<GetCustomListsQuery, Result<IReadOnlyList<CustomMediaListDto>>> _getCustomLists;
    private readonly IQueryHandler<GetCustomListByIdQuery, Result<CustomMediaListDto>> _getCustomListById;
    private readonly IQueryHandler<GetMediaStatsQuery, Result<MediaStatsDto>> _getStats;
    private readonly IQueryHandler<ExportMediaDataQuery, Result<MediaExportDto>> _exportData;
    private readonly IQueryHandler<AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery, Result<IReadOnlyList<TvEpisodeSummaryDto>>> _getSeasonEpisodes;
    private readonly IQueryHandler<AzureTest.Application.Media.Queries.GetSmartRecommendations.GetSmartRecommendationsQuery, Result<MediaRecommendationsDto>> _getRecommendations;

    public MediaQueryUnit(
        IQueryHandler<GetWatchListQuery, Result<PagedResult<WatchItemDto, WatchListFilter>>> getWatchList,
        IQueryHandler<GetWatchItemByIdQuery, Result<WatchItemDto>> getWatchItemById,
        IQueryHandler<GetDiscoverMediaQuery, Result<TmdbDiscoverDto>> getDiscover,
        IQueryHandler<SearchMediaQuery, Result<IReadOnlyList<TmdbSearchResultDto>>> searchMedia,
        IQueryHandler<GetMediaDetailsQuery, Result<TmdbTitleDetailsDto>> getMediaDetails,
        IQueryHandler<GetFollowedPeopleQuery, Result<IReadOnlyList<FollowedPersonDto>>> getFollowedPeople,
        IQueryHandler<GetPersonDetailsQuery, Result<FollowedPersonDto>> getPersonDetails,
        IQueryHandler<GetMediaCalendarQuery, Result<MediaCalendarDto>> getCalendar,
        IQueryHandler<GetMovieNewsQuery, Result<IReadOnlyList<MovieNewsItemDto>>> getNews,
        IQueryHandler<GetCustomListsQuery, Result<IReadOnlyList<CustomMediaListDto>>> getCustomLists,
        IQueryHandler<GetCustomListByIdQuery, Result<CustomMediaListDto>> getCustomListById,
        IQueryHandler<GetMediaStatsQuery, Result<MediaStatsDto>> getStats,
        IQueryHandler<ExportMediaDataQuery, Result<MediaExportDto>> exportData,
        IQueryHandler<AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery, Result<IReadOnlyList<TvEpisodeSummaryDto>>> getSeasonEpisodes,
        IQueryHandler<AzureTest.Application.Media.Queries.GetSmartRecommendations.GetSmartRecommendationsQuery, Result<MediaRecommendationsDto>> getRecommendations)
    {
        _getWatchList = getWatchList;
        _getWatchItemById = getWatchItemById;
        _getDiscover = getDiscover;
        _searchMedia = searchMedia;
        _getMediaDetails = getMediaDetails;
        _getFollowedPeople = getFollowedPeople;
        _getPersonDetails = getPersonDetails;
        _getCalendar = getCalendar;
        _getNews = getNews;
        _getCustomLists = getCustomLists;
        _getCustomListById = getCustomListById;
        _getStats = getStats;
        _exportData = exportData;
        _getSeasonEpisodes = getSeasonEpisodes;
        _getRecommendations = getRecommendations;
    }

    public Task<Result<PagedResult<WatchItemDto, WatchListFilter>>> GetWatchListAsync(GetWatchListQuery query, CancellationToken ct = default) =>
        _getWatchList.HandleAsync(query, ct);

    public Task<Result<WatchItemDto>> GetWatchItemByIdAsync(GetWatchItemByIdQuery query, CancellationToken ct = default) =>
        _getWatchItemById.HandleAsync(query, ct);

    public Task<Result<TmdbDiscoverDto>> GetDiscoverMediaAsync(GetDiscoverMediaQuery query, CancellationToken ct = default) =>
        _getDiscover.HandleAsync(query, ct);

    public Task<Result<IReadOnlyList<TmdbSearchResultDto>>> SearchMediaAsync(SearchMediaQuery query, CancellationToken ct = default) =>
        _searchMedia.HandleAsync(query, ct);

    public Task<Result<TmdbTitleDetailsDto>> GetMediaDetailsAsync(GetMediaDetailsQuery query, CancellationToken ct = default) =>
        _getMediaDetails.HandleAsync(query, ct);

    public Task<Result<IReadOnlyList<FollowedPersonDto>>> GetFollowedPeopleAsync(GetFollowedPeopleQuery query, CancellationToken ct = default) =>
        _getFollowedPeople.HandleAsync(query, ct);

    public Task<Result<FollowedPersonDto>> GetPersonDetailsAsync(GetPersonDetailsQuery query, CancellationToken ct = default) =>
        _getPersonDetails.HandleAsync(query, ct);

    public Task<Result<MediaCalendarDto>> GetMediaCalendarAsync(GetMediaCalendarQuery query, CancellationToken ct = default) =>
        _getCalendar.HandleAsync(query, ct);

    public Task<Result<IReadOnlyList<MovieNewsItemDto>>> GetMovieNewsAsync(GetMovieNewsQuery query, CancellationToken ct = default) =>
        _getNews.HandleAsync(query, ct);

    public Task<Result<IReadOnlyList<CustomMediaListDto>>> GetCustomListsAsync(GetCustomListsQuery query, CancellationToken ct = default) =>
        _getCustomLists.HandleAsync(query, ct);

    public Task<Result<CustomMediaListDto>> GetCustomListByIdAsync(GetCustomListByIdQuery query, CancellationToken ct = default) =>
        _getCustomListById.HandleAsync(query, ct);

    public Task<Result<MediaStatsDto>> GetMediaStatsAsync(GetMediaStatsQuery query, CancellationToken ct = default) =>
        _getStats.HandleAsync(query, ct);

    public Task<Result<MediaExportDto>> ExportMediaDataAsync(ExportMediaDataQuery query, CancellationToken ct = default) =>
        _exportData.HandleAsync(query, ct);

    public Task<Result<IReadOnlyList<TvEpisodeSummaryDto>>> GetSeasonEpisodesAsync(AzureTest.Application.Media.Queries.GetSeasonEpisodes.GetSeasonEpisodesQuery query, CancellationToken ct = default) =>
        _getSeasonEpisodes.HandleAsync(query, ct);

    public Task<Result<MediaRecommendationsDto>> GetSmartRecommendationsAsync(AzureTest.Application.Media.Queries.GetSmartRecommendations.GetSmartRecommendationsQuery query, CancellationToken ct = default) =>
        _getRecommendations.HandleAsync(query, ct);
}
