using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Domain.Entities.FootballFantasy;

namespace AzureTest.Application.Interfaces.Data;

public interface ICountryRepository : IGenericRepository<Country> { }
public interface ILeagueRepository : IGenericRepository<League> { }
public interface ISeasonRepository : IGenericRepository<Season> { }
public interface IStageRepository : IGenericRepository<Stage> { }
public interface ITeamRepository : IGenericRepository<Team> { }
public interface ITeamSeasonRepository : IGenericRepository<TeamSeason> { }
public interface IPlayerRepository : IGenericRepository<Player>
{
    Task<IReadOnlyList<Player>> GetPlayersWithDetailsAsync(CancellationToken cancellationToken = default);
}
public interface IPlayerSquadRepository : IGenericRepository<PlayerSquad> { }
public interface ICoachRepository : IGenericRepository<Coach> { }
public interface ITeamCoachRepository : IGenericRepository<TeamCoach> { }
public interface IFixtureRepository : IGenericRepository<Fixture> { }
public interface IFixtureEventRepository : IGenericRepository<FixtureEvent> { }
public interface IPlayerMatchStatRepository : IGenericRepository<PlayerMatchStat> { }
public interface IPlayerPriceRepository : IGenericRepository<PlayerPrice> { }

public interface IScoringRuleSetRepository : IGenericRepository<ScoringRuleSet> { }
public interface IScoringRuleRepository : IGenericRepository<ScoringRule> { }
public interface IFantasyLeagueRepository : IGenericRepository<FantasyLeague>
{
    Task<FantasyLeague?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FantasyLeague>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
}
public interface IFantasyLeaguePositionLimitRepository : IGenericRepository<FantasyLeaguePositionLimit> { }
public interface IFantasyLeagueSourceRepository : IGenericRepository<FantasyLeagueSource> { }
public interface IFantasyLeaguePoolRuleRepository : IGenericRepository<FantasyLeaguePoolRule> { }
public interface IFantasyLeagueMemberRepository : IGenericRepository<FantasyLeagueMember> { }
public interface IFantasyLeagueInviteRepository : IGenericRepository<FantasyLeagueInvite> { }
public interface IFantasyPeriodRepository : IGenericRepository<FantasyPeriod> { }
public interface IFantasyPeriodFixtureRepository : IGenericRepository<FantasyPeriodFixture> { }
public interface IFantasyTeamRepository : IGenericRepository<FantasyTeam>
{
    Task<FantasyTeam?> GetWithLineupsAndSquadAsync(Guid leagueId, Guid userId, CancellationToken cancellationToken = default);
    Task<FantasyTeam?> GetByIdWithDetailsAsync(Guid teamId, CancellationToken cancellationToken = default);
}
public interface IFantasySquadEntryRepository : IGenericRepository<FantasySquadEntry> { }
public interface IFantasyLineupRepository : IGenericRepository<FantasyLineup> { }
public interface IFantasyLineupSlotRepository : IGenericRepository<FantasyLineupSlot> { }
public interface IFantasyPlayerScoreRepository : IGenericRepository<FantasyPlayerScore> { }
public interface IFantasyTeamPeriodScoreRepository : IGenericRepository<FantasyTeamPeriodScore> { }
public interface IAchievementRepository : IGenericRepository<Achievement> { }
public interface IUserAchievementRepository : IGenericRepository<UserAchievement> { }
public interface IFantasyLeagueGroupRepository : IGenericRepository<FantasyLeagueGroup> { }
public interface IFantasyMatchupRepository : IGenericRepository<FantasyMatchup> { }
public interface IFantasyTeamChipRepository : IGenericRepository<FantasyTeamChip> { }

// Predict Club & Match Stats
public interface IMatchStatRepository : IGenericRepository<MatchStat> { }
public interface IPredictClubRepository : IGenericRepository<PredictClub>
{
    Task<PredictClub?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
public interface IPredictClubMemberRepository : IGenericRepository<PredictClubMember> { }
public interface IMatchPredictionRepository : IGenericRepository<MatchPrediction> { }
public interface ITournamentPredictionRepository : IGenericRepository<TournamentPrediction> { }
