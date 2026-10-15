using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy.Enums;

namespace AzureTest.Contracts.DTOs.FootballFantasy;

// Standings
public record LeagueStandingDto(
    Guid Id,
    Guid SeasonId,
    string SeasonName,
    Guid TeamId,
    string TeamName,
    string? TeamLogo,
    int Rank,
    int Points,
    int GoalsDiff,
    string? Form,
    string? Status,
    string? Description,
    int Played,
    int Win,
    int Draw,
    int Lose,
    int GoalsFor,
    int GoalsAgainst
);

public class LeagueStandingFilter : PagedFilterRequest
{
    public Guid? SeasonId { get; set; }
    public Guid? TeamId { get; set; }
    public LeagueStandingFilter() { SortBy = "Rank"; SortDescending = false; PageSize = 20; }
}

// Bookmakers & Odds
public record BookmakerDto(Guid Id, string Name, string? Url, int OddsCount = 0);

public class BookmakerFilter : PagedFilterRequest
{
    public BookmakerFilter() { SortBy = "Name"; SortDescending = false; PageSize = 20; }
}

public record FixtureOddDto(
    Guid Id,
    Guid FixtureId,
    string FixtureName,
    Guid BookmakerId,
    string BookmakerName,
    OddsMarketType MarketType,
    string ValueLabel,
    decimal Price,
    bool IsLive,
    DateTime UpdatedAt
);

public class FixtureOddFilter : PagedFilterRequest
{
    public Guid? FixtureId { get; set; }
    public Guid? BookmakerId { get; set; }
    public OddsMarketType? MarketType { get; set; }
    public bool? IsLive { get; set; }
    public FixtureOddFilter() { SortBy = "UpdatedAt"; SortDescending = true; PageSize = 20; }
}

public record BetTypeDto(Guid Id, string Name, string Code, string? Description);

// Transfers
public record PlayerTransferDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    DateTime TransferDate,
    TransferType Type,
    Guid? FromTeamId,
    string? FromTeamName,
    Guid? ToTeamId,
    string? ToTeamName,
    string? Fee
);

public class PlayerTransferFilter : PagedFilterRequest
{
    public Guid? PlayerId { get; set; }
    public Guid? TeamId { get; set; }
    public TransferType? Type { get; set; }
    public PlayerTransferFilter() { SortBy = "TransferDate"; SortDescending = true; PageSize = 20; }
}

// Injuries & Sidelined
public record PlayerInjuryDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    Guid? FixtureId,
    string Type,
    string? Reason,
    InjurySeverity Severity,
    DateTime? StartDate,
    DateTime? ExpectedReturn
);

public class PlayerInjuryFilter : PagedFilterRequest
{
    public Guid? PlayerId { get; set; }
    public InjurySeverity? Severity { get; set; }
    public PlayerInjuryFilter() { SortBy = "StartDate"; SortDescending = true; PageSize = 20; }
}

public record SidelinedDto(
    Guid Id,
    Guid? PlayerId,
    string? PlayerName,
    Guid? CoachId,
    string? CoachName,
    string Type,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate
);

public class SidelinedFilter : PagedFilterRequest
{
    public Guid? PlayerId { get; set; }
    public Guid? CoachId { get; set; }
    public SidelinedFilter() { SortBy = "StartDate"; SortDescending = true; PageSize = 20; }
}

// Trophies
public record TeamTrophyDto(
    Guid Id,
    Guid? TeamId,
    string? TeamName,
    Guid? PlayerId,
    string? PlayerName,
    Guid? CoachId,
    string? CoachName,
    string LeagueName,
    string Season,
    string Place
);

public class TeamTrophyFilter : PagedFilterRequest
{
    public Guid? TeamId { get; set; }
    public Guid? PlayerId { get; set; }
    public Guid? CoachId { get; set; }
    public TeamTrophyFilter() { SortBy = "Season"; SortDescending = true; PageSize = 20; }
}

// Head To Head (H2H)
public record HeadToHeadDto(
    Guid Id,
    Guid Team1Id,
    string Team1Name,
    Guid Team2Id,
    string Team2Name,
    int Team1Wins,
    int Team2Wins,
    int Draws,
    int TotalMatches
);

// Team Statistics
public record TeamStatisticDto(
    Guid Id,
    Guid TeamId,
    string TeamName,
    Guid SeasonId,
    string SeasonName,
    string? Form,
    int PlayedTotal,
    int WinsTotal,
    int DrawsTotal,
    int LosesTotal,
    int GoalsForTotal,
    int GoalsAgainstTotal,
    int CleanSheetsTotal,
    int FailedToScoreTotal
);

public class TeamStatisticFilter : PagedFilterRequest
{
    public Guid? TeamId { get; set; }
    public Guid? SeasonId { get; set; }
    public TeamStatisticFilter() { SortBy = "WinsTotal"; SortDescending = true; PageSize = 20; }
}

// Fixture Lineup & Statistics & Live Updates
public record FixtureLineupDto(
    Guid Id,
    Guid FixtureId,
    Guid TeamId,
    string TeamName,
    Guid? CoachId,
    string? CoachName,
    string Formation
);

public record FixtureStatisticDto(
    Guid Id,
    Guid FixtureId,
    Guid TeamId,
    string TeamName,
    string Type,
    string Value
);

public record FixtureLiveUpdateDto(
    Guid Id,
    Guid FixtureId,
    int Minute,
    int? ExtraMinute,
    string Status,
    string? Description,
    DateTime CreatedAt
);

// Player Top Scorers & Season Statistics & Profiles
public record PlayerTopScorerDto(
    Guid Id,
    Guid SeasonId,
    string SeasonName,
    Guid PlayerId,
    string PlayerName,
    Guid TeamId,
    string TeamName,
    int Rank,
    int Goals,
    int Assists,
    int Penalties,
    int PlayedMatches
);

public class PlayerTopScorerFilter : PagedFilterRequest
{
    public Guid? SeasonId { get; set; }
    public PlayerTopScorerFilter() { SortBy = "Rank"; SortDescending = false; PageSize = 20; }
}

public record PlayerSeasonStatisticDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    Guid SeasonId,
    string SeasonName,
    Guid TeamId,
    string TeamName,
    int Appearances,
    int Lineups,
    int Minutes,
    int Goals,
    int Assists,
    int YellowCards,
    int RedCards,
    decimal? Rating
);

public class PlayerSeasonStatisticFilter : PagedFilterRequest
{
    public Guid? SeasonId { get; set; }
    public Guid? PlayerId { get; set; }
    public Guid? TeamId { get; set; }
    public PlayerSeasonStatisticFilter() { SortBy = "Goals"; SortDescending = true; PageSize = 20; }
}

public record PlayerProfileDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    string? Nationality,
    string? BirthPlace,
    string? BirthCountry,
    string? Biography,
    string? Agent,
    decimal? MarketValueEur
);

public record PlayerTeamHistoryDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    Guid TeamId,
    string TeamName,
    DateTime StartDate,
    DateTime? EndDate,
    int Appearances,
    int Goals
);
