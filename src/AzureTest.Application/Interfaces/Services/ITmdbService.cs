using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.Media;
using AzureTest.Domain.Media.Enums;

namespace AzureTest.Application.Interfaces.Services;

public interface ITmdbService
{
    Task<IReadOnlyList<TmdbSearchResultDto>> SearchTitlesAsync(string query, int page = 1, CancellationToken ct = default);
    Task<IReadOnlyList<TmdbSearchResultDto>> SearchPeopleAsync(string query, int page = 1, CancellationToken ct = default);
    Task<TmdbTitleDetailsDto?> GetMovieDetailsAsync(int movieId, CancellationToken ct = default);
    Task<TmdbTitleDetailsDto?> GetTvShowDetailsAsync(int tvShowId, CancellationToken ct = default);
    Task<FollowedPersonDto?> GetPersonDetailsAsync(int personId, CancellationToken ct = default);
    Task<TmdbDiscoverDto> GetDiscoverAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TvEpisodeSummaryDto>> GetTvSeasonEpisodesAsync(int tvShowId, int seasonNumber, CancellationToken ct = default);
    Task<IReadOnlyList<TmdbSearchResultDto>> GetSimilarTitlesAsync(int id, MediaType type, CancellationToken ct = default);
    Task<WatchProvidersDto?> GetWatchProvidersAsync(int id, MediaType type, CancellationToken ct = default);
}
