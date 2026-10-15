using System;
using System.Collections.Generic;

namespace AzureTest.Contracts.DTOs.FootballFantasy;

/// <summary>
/// Configuration model for Football API Providers (e.g. API-Football / RapidAPI).
/// </summary>
public sealed class FootballApiProviderSettingsDto
{
    public string ProviderName { get; set; } = "API-Football";
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://v3.football.api-sports.io";
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastSyncedAt { get; set; }
    public string? LastSyncStatus { get; set; }
    public string? LastSyncMessage { get; set; }

    // Summary counts for synchronizations
    public int CountriesSynced { get; set; }
    public int LeaguesSynced { get; set; }
    public int TeamsSynced { get; set; }
    public int PlayersSynced { get; set; }
    public int FixturesSynced { get; set; }
}

/// <summary>
/// Sync trigger request DTO.
/// </summary>
public sealed class TriggerApiSyncDto
{
    public string SyncType { get; set; } = "all"; // "countries", "leagues", "teams", "players", "fixtures", "all"
    public string? LeagueExternalId { get; set; }
    public int? SeasonYear { get; set; }
}

/// <summary>
/// Result of an external data synchronization.
/// </summary>
public sealed class ApiSyncResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ItemsProcessed { get; set; }
    public int ItemsAdded { get; set; }
    public int ItemsUpdated { get; set; }
    public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
}
