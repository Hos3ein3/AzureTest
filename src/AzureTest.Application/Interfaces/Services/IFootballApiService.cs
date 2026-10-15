using System;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Contracts.DTOs.FootballFantasy;

namespace AzureTest.Application.Interfaces.Services;

/// <summary>
/// Service contract for synchronizing football data with API-Football (v3).
/// </summary>
public interface IFootballApiService
{
    Task<FootballApiProviderSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveSettingsAsync(FootballApiProviderSettingsDto settings, CancellationToken cancellationToken = default);
    Task<ApiSyncResultDto> SyncCountriesAsync(CancellationToken cancellationToken = default);
    Task<ApiSyncResultDto> SyncLeaguesAsync(CancellationToken cancellationToken = default);
    Task<ApiSyncResultDto> SyncTeamsAsync(string? leagueCode = null, CancellationToken cancellationToken = default);
    Task<ApiSyncResultDto> SyncPlayersAsync(string? teamName = null, CancellationToken cancellationToken = default);
    Task<ApiSyncResultDto> SyncFixturesAndStatsAsync(int? seasonYear = null, CancellationToken cancellationToken = default);
}
