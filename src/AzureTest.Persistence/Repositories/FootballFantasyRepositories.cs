using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Repositories;

public class CountryRepository : GenericRepository<Country>, ICountryRepository { public CountryRepository(AzureTestDbContext context) : base(context) { } }
public class LeagueRepository : GenericRepository<League>, ILeagueRepository { public LeagueRepository(AzureTestDbContext context) : base(context) { } }
public class SeasonRepository : GenericRepository<Season>, ISeasonRepository { public SeasonRepository(AzureTestDbContext context) : base(context) { } }
public class StageRepository : GenericRepository<Stage>, IStageRepository { public StageRepository(AzureTestDbContext context) : base(context) { } }
public class TeamRepository : GenericRepository<Team>, ITeamRepository { public TeamRepository(AzureTestDbContext context) : base(context) { } }
public class TeamSeasonRepository : GenericRepository<TeamSeason>, ITeamSeasonRepository { public TeamSeasonRepository(AzureTestDbContext context) : base(context) { } }

public class PlayerRepository : GenericRepository<Player>, IPlayerRepository
{
    public PlayerRepository(AzureTestDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Player>> GetPlayersWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Country)
            .Include(x => x.PlayerSquads)
                .ThenInclude(ps => ps.TeamSeason)
                    .ThenInclude(ts => ts.Team)
            .Include(x => x.PlayerSquads)
                .ThenInclude(ps => ps.TeamSeason)
                    .ThenInclude(ts => ts.Season)
                        .ThenInclude(s => s.League)
            .Include(x => x.Prices)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

public class PlayerSquadRepository : GenericRepository<PlayerSquad>, IPlayerSquadRepository { public PlayerSquadRepository(AzureTestDbContext context) : base(context) { } }
public class CoachRepository : GenericRepository<Coach>, ICoachRepository { public CoachRepository(AzureTestDbContext context) : base(context) { } }
public class TeamCoachRepository : GenericRepository<TeamCoach>, ITeamCoachRepository { public TeamCoachRepository(AzureTestDbContext context) : base(context) { } }
public class FixtureRepository : GenericRepository<Fixture>, IFixtureRepository { public FixtureRepository(AzureTestDbContext context) : base(context) { } }
public class FixtureEventRepository : GenericRepository<FixtureEvent>, IFixtureEventRepository { public FixtureEventRepository(AzureTestDbContext context) : base(context) { } }
public class PlayerMatchStatRepository : GenericRepository<PlayerMatchStat>, IPlayerMatchStatRepository { public PlayerMatchStatRepository(AzureTestDbContext context) : base(context) { } }
public class PlayerPriceRepository : GenericRepository<PlayerPrice>, IPlayerPriceRepository { public PlayerPriceRepository(AzureTestDbContext context) : base(context) { } }

public class ScoringRuleSetRepository : GenericRepository<ScoringRuleSet>, IScoringRuleSetRepository { public ScoringRuleSetRepository(AzureTestDbContext context) : base(context) { } }
public class ScoringRuleRepository : GenericRepository<ScoringRule>, IScoringRuleRepository { public ScoringRuleRepository(AzureTestDbContext context) : base(context) { } }

public class FantasyLeagueRepository : GenericRepository<FantasyLeague>, IFantasyLeagueRepository
{
    public FantasyLeagueRepository(AzureTestDbContext context) : base(context) { }

    public async Task<FantasyLeague?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Members)
            .Include(x => x.PositionLimits)
            .Include(x => x.Sources)
                .ThenInclude(s => s.Season)
                    .ThenInclude(se => se.League)
            .Include(x => x.PoolRules)
            .Include(x => x.Periods)
            .Include(x => x.ScoringRuleSet)
                .ThenInclude(r => r.RulesList)
            .Include(x => x.FantasyTeams)
                .ThenInclude(t => t.SquadEntries)
                    .ThenInclude(se => se.Player)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<FantasyLeague>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Members)
            .Include(x => x.FantasyTeams)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}

public class FantasyLeaguePositionLimitRepository : GenericRepository<FantasyLeaguePositionLimit>, IFantasyLeaguePositionLimitRepository { public FantasyLeaguePositionLimitRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyLeagueSourceRepository : GenericRepository<FantasyLeagueSource>, IFantasyLeagueSourceRepository { public FantasyLeagueSourceRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyLeaguePoolRuleRepository : GenericRepository<FantasyLeaguePoolRule>, IFantasyLeaguePoolRuleRepository { public FantasyLeaguePoolRuleRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyLeagueMemberRepository : GenericRepository<FantasyLeagueMember>, IFantasyLeagueMemberRepository { public FantasyLeagueMemberRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyLeagueInviteRepository : GenericRepository<FantasyLeagueInvite>, IFantasyLeagueInviteRepository { public FantasyLeagueInviteRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyPeriodRepository : GenericRepository<FantasyPeriod>, IFantasyPeriodRepository { public FantasyPeriodRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyPeriodFixtureRepository : GenericRepository<FantasyPeriodFixture>, IFantasyPeriodFixtureRepository { public FantasyPeriodFixtureRepository(AzureTestDbContext context) : base(context) { } }

public class FantasyTeamRepository : GenericRepository<FantasyTeam>, IFantasyTeamRepository
{
    public FantasyTeamRepository(AzureTestDbContext context) : base(context) { }

    public async Task<FantasyTeam?> GetWithLineupsAndSquadAsync(Guid leagueId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.FantasyLeague)
            .Include(x => x.SquadEntries)
                .ThenInclude(s => s.Player)
                    .ThenInclude(p => p.Country)
            .Include(x => x.Lineups)
                .ThenInclude(l => l.Slots)
                    .ThenInclude(sl => sl.Player)
            .FirstOrDefaultAsync(x => x.FantasyLeagueId == leagueId && x.UserId == userId, cancellationToken);
    }

    public async Task<FantasyTeam?> GetByIdWithDetailsAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.FantasyLeague)
            .Include(x => x.SquadEntries)
                .ThenInclude(s => s.Player)
                    .ThenInclude(p => p.Country)
            .Include(x => x.Lineups)
                .ThenInclude(l => l.Slots)
                    .ThenInclude(sl => sl.Player)
            .FirstOrDefaultAsync(x => x.Id == teamId, cancellationToken);
    }
}

public class FantasySquadEntryRepository : GenericRepository<FantasySquadEntry>, IFantasySquadEntryRepository { public FantasySquadEntryRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyLineupRepository : GenericRepository<FantasyLineup>, IFantasyLineupRepository { public FantasyLineupRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyLineupSlotRepository : GenericRepository<FantasyLineupSlot>, IFantasyLineupSlotRepository { public FantasyLineupSlotRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyPlayerScoreRepository : GenericRepository<FantasyPlayerScore>, IFantasyPlayerScoreRepository { public FantasyPlayerScoreRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyTeamPeriodScoreRepository : GenericRepository<FantasyTeamPeriodScore>, IFantasyTeamPeriodScoreRepository { public FantasyTeamPeriodScoreRepository(AzureTestDbContext context) : base(context) { } }
public class AchievementRepository : GenericRepository<Achievement>, IAchievementRepository { public AchievementRepository(AzureTestDbContext context) : base(context) { } }
public class UserAchievementRepository : GenericRepository<UserAchievement>, IUserAchievementRepository { public UserAchievementRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyLeagueGroupRepository : GenericRepository<FantasyLeagueGroup>, IFantasyLeagueGroupRepository { public FantasyLeagueGroupRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyMatchupRepository : GenericRepository<FantasyMatchup>, IFantasyMatchupRepository { public FantasyMatchupRepository(AzureTestDbContext context) : base(context) { } }
public class FantasyTeamChipRepository : GenericRepository<FantasyTeamChip>, IFantasyTeamChipRepository { public FantasyTeamChipRepository(AzureTestDbContext context) : base(context) { } }

// Predict Club Repositories
public class MatchStatRepository : GenericRepository<MatchStat>, IMatchStatRepository { public MatchStatRepository(AzureTestDbContext context) : base(context) { } }

public class PredictClubRepository : GenericRepository<PredictClub>, IPredictClubRepository
{
    public PredictClubRepository(AzureTestDbContext context) : base(context) { }

    public async Task<PredictClub?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(x => x.Members)
            .Include(x => x.Predictions)
                .ThenInclude(p => p.Fixture)
                    .ThenInclude(f => f.HomeTeam)
            .Include(x => x.Predictions)
                .ThenInclude(p => p.Fixture)
                    .ThenInclude(f => f.AwayTeam)
            .Include(x => x.TournamentPredictions)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
}

public class PredictClubMemberRepository : GenericRepository<PredictClubMember>, IPredictClubMemberRepository { public PredictClubMemberRepository(AzureTestDbContext context) : base(context) { } }
public class MatchPredictionRepository : GenericRepository<MatchPrediction>, IMatchPredictionRepository { public MatchPredictionRepository(AzureTestDbContext context) : base(context) { } }
public class TournamentPredictionRepository : GenericRepository<TournamentPrediction>, ITournamentPredictionRepository { public TournamentPredictionRepository(AzureTestDbContext context) : base(context) { } }

