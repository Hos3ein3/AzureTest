using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Application.Interfaces.Data;

/// <summary>
/// Dedicated Read-Only Unit of Work contract serving read query handlers for Football Fantasy and Prediction modules.
/// Bound strictly to ReadOnlyAzureTestDbContext and the replica connection string.
/// </summary>
public interface IReadOnlyFootballFantasyUnitOfWork : IDisposable
{
    ICountryRepository Countries { get; }
    IVenueRepository Venues { get; }
    ILeagueRepository Leagues { get; }
    ISeasonRepository Seasons { get; }
    IStageRepository Stages { get; }
    ITeamRepository Teams { get; }
    ITeamSeasonRepository TeamSeasons { get; }
    IPlayerRepository Players { get; }
    IPlayerSquadRepository PlayerSquads { get; }
    ICoachRepository Coaches { get; }
    ITeamCoachRepository TeamCoaches { get; }
    IFixtureRepository Fixtures { get; }
    IFixtureEventRepository FixtureEvents { get; }
    IPlayerMatchStatRepository PlayerMatchStats { get; }
    IPlayerPriceRepository PlayerPrices { get; }
    IScoringRuleSetRepository ScoringRuleSets { get; }
    IScoringRuleRepository ScoringRules { get; }
    IFantasyLeagueRepository FantasyLeagues { get; }
    IFantasyLeaguePositionLimitRepository FantasyLeaguePositionLimits { get; }
    IFantasyLeagueSourceRepository FantasyLeagueSources { get; }
    IFantasyLeaguePoolRuleRepository FantasyLeaguePoolRules { get; }
    IFantasyLeagueMemberRepository FantasyLeagueMembers { get; }
    IFantasyLeagueInviteRepository FantasyLeagueInvites { get; }
    IFantasyPeriodRepository FantasyPeriods { get; }
    IFantasyPeriodFixtureRepository FantasyPeriodFixtures { get; }
    IFantasyTeamRepository FantasyTeams { get; }
    IFantasySquadEntryRepository FantasySquadEntries { get; }
    IFantasyLineupRepository FantasyLineups { get; }
    IFantasyLineupSlotRepository FantasyLineupSlots { get; }
    IFantasyPlayerScoreRepository FantasyPlayerScores { get; }
    IFantasyTeamPeriodScoreRepository FantasyTeamPeriodScores { get; }
    IAchievementRepository Achievements { get; }
    IUserAchievementRepository UserAchievements { get; }
    IFantasyLeagueGroupRepository FantasyLeagueGroups { get; }
    IFantasyMatchupRepository FantasyMatchups { get; }
    IFantasyTeamChipRepository FantasyTeamChips { get; }

    // Predict Club & Match Stats
    IMatchStatRepository MatchStats { get; }
    IPredictClubRepository PredictClubs { get; }
    IPredictClubMemberRepository PredictClubMembers { get; }
    IMatchPredictionRepository MatchPredictions { get; }
    ITournamentPredictionRepository TournamentPredictions { get; }

    // Extended API-Football Domains
    ILeagueStandingRepository LeagueStandings { get; }
    IBookmakerRepository Bookmakers { get; }
    IFixtureOddRepository FixtureOdds { get; }
    IBetTypeRepository BetTypes { get; }
    IPlayerTransferRepository PlayerTransfers { get; }
    IPlayerInjuryRepository PlayerInjuries { get; }
    ITeamTrophyRepository TeamTrophies { get; }
    ISidelinedRepository Sidelined { get; }
    IHeadToHeadRepository HeadToHeads { get; }
    ITeamStatisticRepository TeamStatistics { get; }
    IFixtureLineupRepository FixtureLineups { get; }
    IFixtureStatisticRepository FixtureStatistics { get; }
    IFixtureLiveUpdateRepository FixtureLiveUpdates { get; }
    IPlayerTopScorerRepository PlayerTopScorers { get; }
    IPlayerSeasonStatisticRepository PlayerSeasonStatistics { get; }
    IPlayerProfileRepository PlayerProfiles { get; }
    IPlayerTeamHistoryRepository PlayerTeamHistories { get; }
}
