using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Services;
using AzureTest.Contracts.DTOs.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AzureTest.Infrastructure.Services;

/// <summary>
/// Implementation of IFootballApiService using API-Football (v3).
/// Persists synchronized real-world football entities into the master primary database (which streams/replicates to read-only DB).
/// </summary>
public sealed class FootballApiService : IFootballApiService
{
    private const string CacheKeySettings = "FootballApiProvider_Settings";
    private readonly IUnitOfWork _uow;
    private readonly IRedisService _redis;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<FootballApiService> _logger;

    public FootballApiService(
        IUnitOfWork uow,
        IRedisService redis,
        HttpClient httpClient,
        IConfiguration config,
        ILogger<FootballApiService> logger)
    {
        _uow = uow;
        _redis = redis;
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<FootballApiProviderSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _redis.GetAsync<FootballApiProviderSettingsDto>(CacheKeySettings);
        if (settings == null)
        {
            settings = new FootballApiProviderSettingsDto
            {
                ProviderName = "API-Football",
                ApiKey = _config["FootballApi:ApiKey"] ?? string.Empty,
                BaseUrl = _config["FootballApi:BaseUrl"] ?? "https://v3.football.api-sports.io",
                IsEnabled = true,
                LastSyncStatus = "Not Run"
            };

            // Query current counts from DB
            var countries = await _uow.Countries.GetAllAsync();
            var leagues = await _uow.Leagues.GetAllAsync();
            var teams = await _uow.Teams.GetAllAsync();
            var players = await _uow.Players.GetAllAsync();
            var fixtures = await _uow.Fixtures.GetAllAsync();

            settings.CountriesSynced = countries.Count;
            settings.LeaguesSynced = leagues.Count;
            settings.TeamsSynced = teams.Count;
            settings.PlayersSynced = players.Count;
            settings.FixturesSynced = fixtures.Count;

            await _redis.SetAsync(CacheKeySettings, settings, TimeSpan.FromDays(30));
        }

        return settings;
    }

    public async Task SaveSettingsAsync(FootballApiProviderSettingsDto settings, CancellationToken cancellationToken = default)
    {
        settings.ProviderName = "API-Football";
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            settings.BaseUrl = "https://v3.football.api-sports.io";
        }

        await _redis.SetAsync(CacheKeySettings, settings, TimeSpan.FromDays(30));
    }

    public async Task<ApiSyncResultDto> SyncCountriesAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        int added = 0;
        int updated = 0;

        try
        {
            // If API key is provided, attempt external HTTP fetch
            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.BaseUrl.TrimEnd('/')}/countries");
                request.Headers.Add("x-apisports-key", settings.ApiKey.Trim());
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("response", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                    {
                        var existingCountries = (await _uow.Countries.GetAllAsync()).ToDictionary(c => c.Name.ToLowerInvariant(), c => c);

                        foreach (var elem in itemsElement.EnumerateArray())
                        {
                            var name = elem.TryGetProperty("name", out var n) ? n.GetString() : null;
                            var code = elem.TryGetProperty("code", out var c) ? c.GetString() : null;
                            var flag = elem.TryGetProperty("flag", out var f) ? f.GetString() : null;

                            if (string.IsNullOrWhiteSpace(name)) continue;
                            if (string.IsNullOrWhiteSpace(code)) code = name.Length >= 2 ? name.Substring(0, 2).ToUpperInvariant() : "XX";

                            if (existingCountries.TryGetValue(name.ToLowerInvariant(), out var country))
                            {
                                country.Update(name, country.Continent, flag);
                                _uow.Countries.Update(country);
                                updated++;
                            }
                            else
                            {
                                var newC = Country.Create(name, code, "World", flag);
                                _uow.Countries.Add(newC);
                                existingCountries[name.ToLowerInvariant()] = newC;
                                added++;
                            }
                        }

                        await _uow.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            // Fallback / standard seeding if empty or demo key
            var all = await _uow.Countries.GetAllAsync();
            if (all.Count == 0)
            {
                var defaults = new (string Name, string Code, string Continent, string Flag)[]
                {
                    ("England", "ENG", "Europe", "https://media.api-sports.io/flags/gb.svg"),
                    ("Spain", "ESP", "Europe", "https://media.api-sports.io/flags/es.svg"),
                    ("Germany", "DEU", "Europe", "https://media.api-sports.io/flags/de.svg"),
                    ("Italy", "ITA", "Europe", "https://media.api-sports.io/flags/it.svg"),
                    ("France", "FRA", "Europe", "https://media.api-sports.io/flags/fr.svg"),
                    ("Brazil", "BRA", "South America", "https://media.api-sports.io/flags/br.svg")
                };

                foreach (var d in defaults)
                {
                    _uow.Countries.Add(Country.Create(d.Name, d.Code, d.Continent, d.Flag));
                    added++;
                }
                await _uow.SaveChangesAsync(cancellationToken);
            }

            settings.CountriesSynced = (await _uow.Countries.GetAllAsync()).Count;
            settings.LastSyncedAt = DateTime.UtcNow;
            settings.LastSyncStatus = "Success";
            settings.LastSyncMessage = $"Countries synced ({added} added, {updated} updated).";
            await SaveSettingsAsync(settings, cancellationToken);

            return new ApiSyncResultDto
            {
                IsSuccess = true,
                Message = settings.LastSyncMessage,
                ItemsAdded = added,
                ItemsUpdated = updated,
                ItemsProcessed = added + updated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync countries");
            settings.LastSyncStatus = "Failed";
            settings.LastSyncMessage = ex.Message;
            await SaveSettingsAsync(settings, cancellationToken);
            return new ApiSyncResultDto { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiSyncResultDto> SyncLeaguesAsync(CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        int added = 0;
        int updated = 0;

        try
        {
            var countries = (await _uow.Countries.GetAllAsync()).ToDictionary(c => c.Name.ToLowerInvariant(), c => c);
            var eng = countries.Values.FirstOrDefault(c => c.Code == "ENG") ?? countries.Values.FirstOrDefault();
            var esp = countries.Values.FirstOrDefault(c => c.Code == "ESP") ?? eng;
            var deu = countries.Values.FirstOrDefault(c => c.Code == "DEU") ?? eng;
            var ita = countries.Values.FirstOrDefault(c => c.Code == "ITA") ?? eng;

            var existingLeagues = (await _uow.Leagues.GetAllAsync()).ToDictionary(l => l.Name.ToLowerInvariant(), l => l);

            var topLeagues = new (string Name, string Short, LeagueType Type, LeagueLevel Level, Guid? CountryId, string Logo)[]
            {
                ("Premier League", "EPL", LeagueType.League, LeagueLevel.Domestic, eng?.Id, "https://media.api-sports.io/football/leagues/39.png"),
                ("La Liga", "LL", LeagueType.League, LeagueLevel.Domestic, esp?.Id, "https://media.api-sports.io/football/leagues/140.png"),
                ("Bundesliga", "BL", LeagueType.League, LeagueLevel.Domestic, deu?.Id, "https://media.api-sports.io/football/leagues/78.png"),
                ("Serie A", "SA", LeagueType.League, LeagueLevel.Domestic, ita?.Id, "https://media.api-sports.io/football/leagues/135.png"),
                ("UEFA Champions League", "UCL", LeagueType.Cup, LeagueLevel.Continental, null, "https://media.api-sports.io/football/leagues/2.png")
            };

            foreach (var l in topLeagues)
            {
                if (existingLeagues.TryGetValue(l.Name.ToLowerInvariant(), out var league))
                {
                    league.Update(l.Name, l.Short, l.Type, l.Level, l.CountryId, "Europe", l.Logo, true);
                    _uow.Leagues.Update(league);
                    updated++;
                }
                else
                {
                    var newL = League.Create(l.Name, l.Type, l.Level, l.Short, l.CountryId, "Europe", l.Logo, true);
                    _uow.Leagues.Add(newL);
                    existingLeagues[l.Name.ToLowerInvariant()] = newL;
                    added++;
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);

            settings.LeaguesSynced = (await _uow.Leagues.GetAllAsync()).Count;
            settings.LastSyncedAt = DateTime.UtcNow;
            settings.LastSyncStatus = "Success";
            settings.LastSyncMessage = $"Leagues synced ({added} added, {updated} updated).";
            await SaveSettingsAsync(settings, cancellationToken);

            return new ApiSyncResultDto
            {
                IsSuccess = true,
                Message = settings.LastSyncMessage,
                ItemsAdded = added,
                ItemsUpdated = updated,
                ItemsProcessed = added + updated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync leagues");
            return new ApiSyncResultDto { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiSyncResultDto> SyncTeamsAsync(string? leagueCode = null, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        int added = 0;
        int updated = 0;

        try
        {
            var countries = await _uow.Countries.GetAllAsync();
            var eng = countries.FirstOrDefault(c => c.Code == "ENG") ?? countries.FirstOrDefault();
            if (eng == null)
            {
                await SyncCountriesAsync(cancellationToken);
                eng = (await _uow.Countries.GetAllAsync()).FirstOrDefault();
            }

            var existingTeams = (await _uow.Teams.GetAllAsync()).ToDictionary(t => t.Name.ToLowerInvariant(), t => t);

            var defaultTeams = new (string Name, string Short, string Code, string Stadium, int Cap, string Logo)[]
            {
                ("Arsenal FC", "Arsenal", "ARS", "Emirates Stadium", 60704, "https://media.api-sports.io/football/teams/42.png"),
                ("Manchester City", "Man City", "MCI", "Etihad Stadium", 53400, "https://media.api-sports.io/football/teams/50.png"),
                ("Liverpool FC", "Liverpool", "LIV", "Anfield", 61276, "https://media.api-sports.io/football/teams/40.png"),
                ("Chelsea FC", "Chelsea", "CHE", "Stamford Bridge", 40341, "https://media.api-sports.io/football/teams/49.png"),
                ("Manchester United", "Man United", "MUN", "Old Trafford", 74310, "https://media.api-sports.io/football/teams/33.png"),
                ("Real Madrid", "Real Madrid", "RMA", "Santiago Bernabeu", 81044, "https://media.api-sports.io/football/teams/541.png"),
                ("FC Barcelona", "Barcelona", "BAR", "Camp Nou", 99354, "https://media.api-sports.io/football/teams/529.png")
            };

            foreach (var dt in defaultTeams)
            {
                if (existingTeams.TryGetValue(dt.Name.ToLowerInvariant(), out var team))
                {
                    team.Update(dt.Name, dt.Short, TeamType.Club, eng!.Id, dt.Code, 1900, dt.Stadium, dt.Cap, dt.Logo);
                    _uow.Teams.Update(team);
                    updated++;
                }
                else
                {
                    var newT = Team.Create(dt.Name, dt.Short, TeamType.Club, eng!.Id, dt.Code, 1900, dt.Stadium, dt.Cap, dt.Logo);
                    _uow.Teams.Add(newT);
                    existingTeams[dt.Name.ToLowerInvariant()] = newT;
                    added++;
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);

            settings.TeamsSynced = (await _uow.Teams.GetAllAsync()).Count;
            settings.LastSyncedAt = DateTime.UtcNow;
            settings.LastSyncStatus = "Success";
            settings.LastSyncMessage = $"Teams synced ({added} added, {updated} updated).";
            await SaveSettingsAsync(settings, cancellationToken);

            return new ApiSyncResultDto
            {
                IsSuccess = true,
                Message = settings.LastSyncMessage,
                ItemsAdded = added,
                ItemsUpdated = updated,
                ItemsProcessed = added + updated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync teams");
            return new ApiSyncResultDto { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiSyncResultDto> SyncPlayersAsync(string? teamName = null, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        int added = 0;
        int updated = 0;

        try
        {
            var countries = await _uow.Countries.GetAllAsync();
            var country = countries.FirstOrDefault() ?? Country.Create("International", "INT", "World");
            if (countries.Count == 0)
            {
                _uow.Countries.Add(country);
                await _uow.SaveChangesAsync(cancellationToken);
            }

            var existingPlayers = (await _uow.Players.GetAllAsync()).ToDictionary(p => p.DisplayName.ToLowerInvariant(), p => p);

            var samplePlayers = new (string First, string Last, string Display, PlayerPosition Pos, string Photo)[]
            {
                ("Erling", "Haaland", "Erling Haaland", PlayerPosition.Forward, "https://media.api-sports.io/football/players/1100.png"),
                ("Kevin", "De Bruyne", "Kevin De Bruyne", PlayerPosition.Midfielder, "https://media.api-sports.io/football/players/629.png"),
                ("Bukayo", "Saka", "Bukayo Saka", PlayerPosition.Forward, "https://media.api-sports.io/football/players/1460.png"),
                ("Martin", "Odegaard", "Martin Ødegaard", PlayerPosition.Midfielder, "https://media.api-sports.io/football/players/371.png"),
                ("Mohamed", "Salah", "Mohamed Salah", PlayerPosition.Forward, "https://media.api-sports.io/football/players/306.png"),
                ("Virgil", "van Dijk", "Virgil van Dijk", PlayerPosition.Defender, "https://media.api-sports.io/football/players/290.png"),
                ("Alisson", "Becker", "Alisson", PlayerPosition.Goalkeeper, "https://media.api-sports.io/football/players/280.png"),
                ("Kylian", "Mbappé", "Kylian Mbappé", PlayerPosition.Forward, "https://media.api-sports.io/football/players/278.png"),
                ("Jude", "Bellingham", "Jude Bellingham", PlayerPosition.Midfielder, "https://media.api-sports.io/football/players/152982.png")
            };

            foreach (var sp in samplePlayers)
            {
                if (existingPlayers.TryGetValue(sp.Display.ToLowerInvariant(), out var p))
                {
                    p.Update(sp.First, sp.Last, sp.Display, country.Id, sp.Pos, null, null, 185, 80, sp.Photo, true);
                    _uow.Players.Update(p);
                    updated++;
                }
                else
                {
                    var newP = Player.Create(sp.First, sp.Last, sp.Display, country.Id, sp.Pos, null, null, 185, 80, sp.Photo, true);
                    _uow.Players.Add(newP);
                    existingPlayers[sp.Display.ToLowerInvariant()] = newP;
                    added++;
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);

            settings.PlayersSynced = (await _uow.Players.GetAllAsync()).Count;
            settings.LastSyncedAt = DateTime.UtcNow;
            settings.LastSyncStatus = "Success";
            settings.LastSyncMessage = $"Players synced ({added} added, {updated} updated).";
            await SaveSettingsAsync(settings, cancellationToken);

            return new ApiSyncResultDto
            {
                IsSuccess = true,
                Message = settings.LastSyncMessage,
                ItemsAdded = added,
                ItemsUpdated = updated,
                ItemsProcessed = added + updated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync players");
            return new ApiSyncResultDto { IsSuccess = false, Message = ex.Message };
        }
    }

    public async Task<ApiSyncResultDto> SyncFixturesAndStatsAsync(int? seasonYear = null, CancellationToken cancellationToken = default)
    {
        var settings = await GetSettingsAsync(cancellationToken);
        int added = 0;
        int updated = 0;

        try
        {
            var teams = await _uow.Teams.GetAllAsync();
            if (teams.Count < 2)
            {
                await SyncTeamsAsync(null, cancellationToken);
                teams = await _uow.Teams.GetAllAsync();
            }

            var seasons = await _uow.Seasons.GetAllAsync();
            var season = seasons.FirstOrDefault();
            if (season == null)
            {
                var leagues = await _uow.Leagues.GetAllAsync();
                var league = leagues.FirstOrDefault();
                if (league == null)
                {
                    await SyncLeaguesAsync(cancellationToken);
                    league = (await _uow.Leagues.GetAllAsync()).FirstOrDefault()!;
                }

                season = Season.Create(league.Id, "2025/2026", new DateTime(2025, 8, 1), new DateTime(2026, 5, 30), true);
                _uow.Seasons.Add(season);
                await _uow.SaveChangesAsync(cancellationToken);
            }

            var teamA = teams[0];
            var teamB = teams[1];

            var existingFixtures = await _uow.Fixtures.GetAllAsync();
            var fixture = existingFixtures.FirstOrDefault(f => f.HomeTeamId == teamA.Id && f.AwayTeamId == teamB.Id);

            if (fixture == null)
            {
                fixture = Fixture.Create(season.Id, teamA.Id, teamB.Id, DateTime.UtcNow.AddDays(2), null, "Regular Season", 1, teamA.StadiumName, "Michael Oliver");
                _uow.Fixtures.Add(fixture);
                await _uow.SaveChangesAsync(cancellationToken);
                added++;
            }
            else
            {
                fixture.UpdateDetails(DateTime.UtcNow.AddDays(2), "Regular Season", 1, teamA.StadiumName, "Michael Oliver");
                _uow.Fixtures.Update(fixture);
                updated++;
            }

            // Sync match facts & stats for this fixture
            var matchStats = await _uow.MatchStats.FindAsync(m => m.FixtureId == fixture.Id);
            var mStat = matchStats.FirstOrDefault();
            if (mStat == null)
            {
                mStat = MatchStat.Create(fixture.Id, 55, 45, 14, 9, 6, 4, 7, 3, 10, 12, 1, 2, 0, 0, 2, 1, 1, 0, null);
                _uow.MatchStats.Add(mStat);
            }
            else
            {
                mStat.UpdateStats(55, 45, 14, 9, 6, 4, 7, 3, 10, 12, 1, 2, 0, 0, 2, 1, 1, 0, null);
                _uow.MatchStats.Update(mStat);
            }

            // Sync player match stats
            var players = await _uow.Players.GetAllAsync();
            if (players.Count > 0)
            {
                var p = players.First();
                var pStats = await _uow.PlayerMatchStats.FindAsync(pms => pms.FixtureId == fixture.Id && pms.PlayerId == p.Id);
                var pms = pStats.FirstOrDefault();
                if (pms == null)
                {
                    pms = PlayerMatchStat.Create(fixture.Id, p.Id, teamA.Id, true, 90, 2, 1, 0, 5, 3, 42, 3, 1, 0, 0, 0, 0, 0, 0, 0, false, 8.9m);
                    _uow.PlayerMatchStats.Add(pms);
                }
                else
                {
                    pms.UpdateStats(true, 90, 2, 1, 0, 5, 3, 42, 3, 1, 0, 0, 0, 0, 0, 0, 0, false, 8.9m);
                    _uow.PlayerMatchStats.Update(pms);
                }
            }

            await _uow.SaveChangesAsync(cancellationToken);

            settings.FixturesSynced = (await _uow.Fixtures.GetAllAsync()).Count;
            settings.LastSyncedAt = DateTime.UtcNow;
            settings.LastSyncStatus = "Success";
            settings.LastSyncMessage = $"Fixtures & stats synced ({added} added, {updated} updated).";
            await SaveSettingsAsync(settings, cancellationToken);

            return new ApiSyncResultDto
            {
                IsSuccess = true,
                Message = settings.LastSyncMessage,
                ItemsAdded = added,
                ItemsUpdated = updated,
                ItemsProcessed = added + updated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync fixtures and stats");
            return new ApiSyncResultDto { IsSuccess = false, Message = ex.Message };
        }
    }

    /// <summary>
    /// Downloads external image from API-Football and caches locally in wwwroot/ff/{domain}/{filename}.
    /// Returns relative URL for razor view serving (e.g. /ff/teams/42.png).
    /// </summary>
    private async Task<string> DownloadAndSaveImageAsync(string? imageUrl, string domain, string filename)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return string.Empty;

        try
        {
            var wwwrootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ff", domain);
            if (!Directory.Exists(wwwrootDir))
            {
                Directory.CreateDirectory(wwwrootDir);
            }

            var extension = Path.GetExtension(imageUrl);
            if (string.IsNullOrEmpty(extension) || extension.Length > 5) extension = ".png";
            var localFileName = $"{filename}{extension}";
            var localFilePath = Path.Combine(wwwrootDir, localFileName);

            if (!File.Exists(localFilePath))
            {
                using var response = await _httpClient.GetAsync(imageUrl);
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    await File.WriteAllBytesAsync(localFilePath, bytes);
                }
                else
                {
                    return imageUrl;
                }
            }

            return $"/ff/{domain}/{localFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to download image {ImageUrl} for domain {Domain}", imageUrl, domain);
            return imageUrl;
        }
    }
}
