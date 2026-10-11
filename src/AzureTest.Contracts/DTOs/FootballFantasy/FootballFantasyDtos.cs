using System;
using System.Collections.Generic;
using AzureTest.Contracts.DTOs.Common;
using AzureTest.Domain.Entities.FootballFantasy.Enums;

namespace AzureTest.Contracts.DTOs.FootballFantasy;

// Real-world DTOs
public record CountryDto(Guid Id, string Name, string Code, string Continent, string? FlagUrl);
public record LeagueDto(Guid Id, string Name, string? ShortName, LeagueType Type, LeagueLevel Level, string? CountryName, string? Continent, string? LogoUrl, bool IsActive);
public record SeasonDto(Guid Id, Guid LeagueId, string LeagueName, string Name, DateTime StartDate, DateTime EndDate, bool IsCurrent);
public record TeamDto(Guid Id, string Name, string ShortName, string? Code, TeamType TeamType, Guid CountryId, string CountryName, int? FoundedYear, string? StadiumName, int? StadiumCapacity, string? LogoUrl);
public record PlayerDto(
    Guid Id,
    string FirstName,
    string LastName,
    string DisplayName,
    DateTime? DateOfBirth,
    Guid CountryId,
    string CountryName,
    PlayerPosition Position,
    PreferredFoot? PreferredFoot,
    int? HeightCm,
    int? WeightKg,
    string? PhotoUrl,
    bool IsActive,
    string? CurrentTeamName,
    decimal CurrentPrice,
    int TotalSeasonPoints = 0
);
public record FixtureDto(
    Guid Id,
    Guid SeasonId,
    string HomeTeamName,
    string? HomeTeamLogo,
    string AwayTeamName,
    string? AwayTeamLogo,
    DateTime KickoffAt,
    FixtureStatus Status,
    int? HomeScore,
    int? AwayScore,
    int? Minute,
    string? Round,
    string? Venue,
    bool IsStatsFinalized
);

public record ScoringRuleItemDto(
    AzureTest.Domain.Entities.FootballFantasy.Enums.ScoringMetric Metric,
    AzureTest.Domain.Entities.FootballFantasy.Enums.PlayerPosition? Position,
    int Points,
    int? Threshold = null
);

// Fantasy League DTOs
public record FantasyLeagueDto(
    Guid Id,
    Guid? OwnerUserId,
    string Name,
    string LeagueTag,
    string? Description,
    FantasyLeagueVisibility Visibility,
    FantasyLeagueMode Mode,
    FantasyPeriodType PeriodType,
    FantasySquadMode SquadMode,
    int SquadSize,
    int StartingSize,
    int BenchSize,
    decimal Budget,
    int? MaxPlayersPerRealTeam,
    string? AllowedFormations,
    bool CaptainEnabled,
    decimal CaptainMultiplier,
    int? MaxMembers,
    int MemberCount,
    string? JoinCode,
    Guid ScoringRuleSetId,
    DateTime StartsAt,
    DateTime EndsAt,
    DateTime? CloseTime,
    FantasyLeagueStatus Status,
    bool UserHasTeam = false,
    string? PoolScopeSummary = null,
    PoolScopeType? PoolScopeType = null,
    string? PoolScopeValue = null,
    List<ScoringRuleItemDto>? ScoringRules = null,
    string? LogoUrl = null,
    bool AllowCoach = true,
    DateTime? Window1Open = null,
    DateTime? Window1Close = null,
    DateTime? Window2Open = null,
    DateTime? Window2Close = null,
    int MinSquadSize = 11,
    FantasyKickoutType KickoutType = FantasyKickoutType.FullRemoval,
    DateTime? AuctionStartsAt = null,
    DateTime? AuctionEndsAt = null,
    FantasyLeagueFormat Format = FantasyLeagueFormat.TotalPoints,
    bool AllowAutoSub = false,
    bool EnableMvpBonus = false,
    int MvpBonusPoints = 3,
    int? GroupCount = null
);

// Filters
public class FantasyLeagueFilter : PagedFilterRequest
{
    public FantasyLeagueVisibility? Visibility { get; set; }
    public FantasyLeagueStatus? Status { get; set; }

    public FantasyLeagueFilter()
    {
        SortBy = "StartsAt";
        SortDescending = false;
        PageSize = 12;
    }
}

public class PlayerFilter : PagedFilterRequest
{
    public Guid? FantasyLeagueId { get; set; }
    public PlayerPosition? Position { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? LeagueId { get; set; }
    public decimal? MaxPrice { get; set; }

    public PlayerFilter()
    {
        SortBy = "DisplayName";
        SortDescending = false;
        PageSize = 20;
    }
}

public record CoachDto(Guid Id, string FirstName, string LastName, Guid CountryId, string CountryName, DateTime? DateOfBirth, string? PhotoUrl)
{
    public string FullName => $"{FirstName} {LastName}".Trim();
}

public record PlayerMatchStatDto(
    Guid Id,
    Guid FixtureId,
    string FixtureName,
    Guid PlayerId,
    string PlayerName,
    Guid TeamId,
    string TeamName,
    bool IsStarter,
    int MinutesPlayed,
    int Goals,
    int Assists,
    int OwnGoals,
    int ShotsTotal,
    int ShotsOnTarget,
    int PassesTotal,
    int KeyPasses,
    int Tackles,
    int Interceptions,
    int Saves,
    int GoalsConceded,
    int PenaltiesSaved,
    int PenaltiesMissed,
    int YellowCards,
    int RedCards,
    bool CleanSheet,
    decimal? ProviderRating
);

public class CountryFilter : PagedFilterRequest
{
    public string? Continent { get; set; }
    public CountryFilter() { SortBy = "Name"; SortDescending = false; PageSize = 20; }
}

public class LeagueFilter : PagedFilterRequest
{
    public LeagueType? Type { get; set; }
    public Guid? CountryId { get; set; }
    public LeagueFilter() { SortBy = "Name"; SortDescending = false; PageSize = 20; }
}

public class TeamFilter : PagedFilterRequest
{
    public Guid? CountryId { get; set; }
    public TeamFilter() { SortBy = "Name"; SortDescending = false; PageSize = 20; }
}

public class FixtureFilter : PagedFilterRequest
{
    public Guid? SeasonId { get; set; }
    public FixtureStatus? Status { get; set; }
    public FixtureFilter() { SortBy = "KickoffAt"; SortDescending = false; PageSize = 20; }
}

public class CoachFilter : PagedFilterRequest
{
    public Guid? CountryId { get; set; }
    public CoachFilter() { SortBy = "LastName"; SortDescending = false; PageSize = 20; }
}

public class PlayerMatchStatFilter : PagedFilterRequest
{
    public Guid? FixtureId { get; set; }
    public Guid? PlayerId { get; set; }
    public Guid? TeamId { get; set; }
    public PlayerMatchStatFilter() { SortBy = "MinutesPlayed"; SortDescending = true; PageSize = 20; }
}

public record AdminFantasyTeamOverviewDto(
    Guid Id,
    string TeamName,
    string? LogoUrl,
    Guid FantasyLeagueId,
    string LeagueName,
    string LeagueTag,
    Guid UserId,
    string ManagerName,
    int TotalPoints,
    decimal CreditsLeft,
    int SquadCount,
    DateTime JoinedAt
);


// Fantasy Team DTOs
public record FantasyTeamDto(
    Guid Id,
    Guid FantasyLeagueId,
    string FantasyLeagueName,
    Guid UserId,
    string Name,
    string? LogoUrl,
    decimal CreditsLeft,
    int TotalPoints,
    int Rank = 1,
    Guid? CoachId = null,
    string? CoachName = null
);

public record FantasySquadEntryDto(
    Guid Id,
    Guid FantasyTeamId,
    Guid PlayerId,
    string PlayerName,
    PlayerPosition Position,
    string? CurrentTeamName,
    decimal PricePaid,
    string? PhotoUrl
);

public record FantasyLineupDto(
    Guid Id,
    Guid FantasyTeamId,
    Guid FantasyPeriodId,
    string Formation,
    Guid? CaptainPlayerId,
    Guid? ViceCaptainPlayerId,
    bool IsLocked,
    List<FantasyLineupSlotDto> Slots
);

public record FantasyLineupSlotDto(
    Guid Id,
    Guid PlayerId,
    string PlayerName,
    PlayerPosition Position,
    string? CurrentTeamName,
    string? PhotoUrl,
    bool IsStarter,
    int? BenchOrder,
    bool IsCaptain,
    bool IsViceCaptain
);

// Create / Mutation requests
public record CreateFantasyTeamRequest(
    Guid FantasyLeagueId,
    string Name,
    string? LogoUrl,
    Guid? CoachId = null
);

public record SaveLineupRequest(
    Guid FantasyTeamId,
    Guid FantasyPeriodId,
    string Formation,
    Guid? CaptainPlayerId,
    Guid? ViceCaptainPlayerId,
    List<LineupSlotRequest> Slots,
    Guid? CoachId = null
);

public record LineupSlotRequest(
    Guid PlayerId,
    bool IsStarter,
    int? BenchOrder
);

public record JoinLeagueRequest(
    Guid FantasyLeagueId,
    string TeamName,
    string? JoinCode,
    string? LogoUrl,
    Guid? CoachId = null
);

// Achievement DTOs
public record AchievementDto(
    Guid Id,
    string Title,
    string Description,
    string BadgeIcon,
    int PointsReward,
    bool IsActive
);

public class AchievementFilter : PagedFilterRequest
{
    public bool? IsActive { get; set; }
    public AchievementFilter() { SortBy = "PointsReward"; SortDescending = true; PageSize = 20; }
}

public record UserAchievementDto(
    Guid Id,
    Guid UserId,
    Guid AchievementId,
    string Title,
    string Description,
    string BadgeIcon,
    int PointsReward,
    DateTime UnlockedAt
);

// Matchup and Group DTOs (H2H, Knockout, Grouped)
public record FantasyMatchupDto(
    Guid Id,
    Guid FantasyLeagueId,
    Guid? FantasyLeagueGroupId,
    string? GroupName,
    Guid? PeriodId,
    int RoundNumber,
    string StageName,
    int MatchNumber,
    Guid? HomeTeamId,
    string? HomeTeamName,
    decimal HomeScore,
    Guid? AwayTeamId,
    string? AwayTeamName,
    decimal AwayScore,
    Guid? WinnerTeamId,
    string? WinnerTeamName,
    bool IsCompleted
);

public record FantasyLeagueGroupDto(
    Guid Id,
    Guid FantasyLeagueId,
    string Name,
    int GroupNumber,
    List<FantasyTeamDto> Teams,
    List<FantasyMatchupDto> Matchups
);

public record ManagerLeagueHistoryDto(
    Guid LeagueId,
    string LeagueName,
    string LeagueTag,
    FantasyMemberRole Role,
    FantasyMemberStatus Status,
    DateTime JoinedAt,
    bool HasLeft
);

public record PredictionProfileSummaryDto(
    int TotalPredictionPoints,
    int TotalPredictionsMade,
    int CorrectExactScores,
    int CorrectOutcomes,
    int PredictClubsCount,
    List<PredictClubMemberDto> ClubMemberships
);

// Manager Profile DTO
public record ManagerProfileDto(
    Guid UserId,
    string ManagerName,
    string Email,
    int TotalCareerPoints,
    int TotalTeamsCreated,
    int TotalTrophies,
    List<FantasyTeamDto> ActiveTeams,
    List<UserAchievementDto> Achievements,
    List<ManagerLeagueHistoryDto> LeagueHistory,
    PredictionProfileSummaryDto? PredictionSummary = null
);

// Match Facts & Stats DTO
public record MatchStatDto(
    Guid Id,
    Guid FixtureId,
    Guid? MvpPlayerId,
    string? MvpPlayerName,
    int HomePossession,
    int AwayPossession,
    int HomeShotsTotal,
    int AwayShotsTotal,
    int HomeShotsOnTarget,
    int AwayShotsOnTarget,
    int HomeCorners,
    int AwayCorners,
    int HomeFouls,
    int AwayFouls,
    int HomeYellowCards,
    int AwayYellowCards,
    int HomeRedCards,
    int AwayRedCards,
    int HomeOffsides,
    int AwayOffsides,
    int? HomeHalfTimeScore,
    int? AwayHalfTimeScore
);

// Predict Club DTOs
public record PredictClubDto(
    Guid Id,
    Guid? OwnerUserId,
    string Name,
    string ClubTag,
    string? Description,
    string? LogoUrl,
    FantasyLeagueVisibility Visibility,
    FantasyLeagueFormat Format,
    PoolScopeType ScopeType,
    string ScopeValue,
    int? MaxMembers,
    int MemberCount,
    string? JoinCode,
    PredictClubStatus Status,
    DateTime StartsAt,
    DateTime EndsAt,
    int ExactScorePoints,
    int OutcomePoints,
    int HalfTimeScorePoints,
    int BothTeamsToScorePoints,
    int OverUnderPoints,
    int YellowCardsPoints,
    int CornersPoints,
    int MatchMvpPoints,
    int FirstGoalMinutePoints,
    bool EnableTournamentPredictions,
    int TournamentPodiumPoints,
    int TournamentAwardPoints,
    bool PredictExactScore,
    bool PredictOutcome,
    bool PredictHalfTime,
    bool PredictBtts,
    bool PredictCards,
    bool PredictCorners,
    bool PredictMvp
);

public record PredictClubMemberDto(
    Guid Id,
    Guid PredictClubId,
    Guid UserId,
    string UserName,
    FantasyMemberRole Role,
    FantasyMemberStatus Status,
    DateTime JoinedAt,
    int TotalPoints,
    int CorrectExactScores,
    int CorrectOutcomes,
    int TotalPredictionsCount,
    int Rank = 0
);

public record MatchPredictionDto(
    Guid Id,
    Guid PredictClubId,
    Guid FixtureId,
    string HomeTeamName,
    string? HomeTeamLogo,
    string AwayTeamName,
    string? AwayTeamLogo,
    DateTime KickoffAt,
    string? Round,
    int? ActualHomeScore,
    int? ActualAwayScore,
    FixtureStatus FixtureStatus,
    Guid UserId,
    PredictionEntryStatus Status,
    int? PredictedHomeScore,
    int? PredictedAwayScore,
    int? PredictedHalfTimeHomeScore,
    int? PredictedHalfTimeAwayScore,
    bool? BothTeamsToScore,
    decimal? TotalGoalsOverUnder,
    int? PredictedTotalYellowCards,
    int? PredictedTotalCorners,
    int? PredictedFirstGoalMinute,
    Guid? PredictedMvpPlayerId,
    string? PredictedMvpPlayerName,
    int PointsEarned,
    bool IsScored,
    bool CanEdit
);

public record TournamentPredictionDto(
    Guid Id,
    Guid PredictClubId,
    Guid UserId,
    PredictionEntryStatus Status,
    Guid? FirstPlaceTeamId,
    string? FirstPlaceTeamName,
    Guid? SecondPlaceTeamId,
    string? SecondPlaceTeamName,
    Guid? ThirdPlaceTeamId,
    string? ThirdPlaceTeamName,
    Guid? BestPlayerId,
    string? BestPlayerName,
    Guid? TopScorerPlayerId,
    string? TopScorerPlayerName,
    Guid? TopAssisterPlayerId,
    string? TopAssisterPlayerName,
    int PointsEarned,
    bool IsScored
);

public class PredictClubFilter : PagedFilterRequest
{
    public FantasyLeagueVisibility? Visibility { get; set; }
    public PredictClubStatus? Status { get; set; }
    public PoolScopeType? ScopeType { get; set; }
    public string? ScopeValue { get; set; }
    public Guid? UserId { get; set; }

    public PredictClubFilter()
    {
        SortBy = "CreatedOn";
        SortDescending = true;
        PageSize = 12;
    }
}
