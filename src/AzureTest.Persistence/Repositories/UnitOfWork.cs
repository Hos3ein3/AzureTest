

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AzureTest.Application.Interfaces.Data;
using AzureTest.Application.Interfaces.Data.Crypto;
using AzureTest.Application.Interfaces.Data.Outbox;
using AzureTest.Application.Interfaces.Data.Trading;
using AzureTest.Domain.Primitives;
using AzureTest.Persistence.Context;
using AzureTest.Persistence.Repositories.FootballFantasy;

namespace AzureTest.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AzureTestDbContext _dbContext;

    private IEventRepository? _events;
    private ITaskRepository? _tasks;
    private IHabitTrackerRepository? _habits;
    private IHabitLogRepository? _habitLogs;
    private ITicketRepository? _tickets;
    private ITicketMessageRepository? _ticketMessages;
    private IResumeRepository? _resumes;
    private IDebtRepository? _debts;
    private IExpenseRepository? _expenses;

    private IPermissionRepository? _permissions;
    private IRolePermissionRepository? _rolePermissions;
    private IUserPermissionRepository? _userPermissions;
    private IPomodoroRepository? _pomodoros;

    public UnitOfWork(AzureTestDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public IEventRepository Events => _events ??= new EventRepository(_dbContext);
    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_dbContext);
    public IHabitTrackerRepository Habits => _habits ??= new HabitTrackerRepository(_dbContext);
    public IHabitLogRepository HabitLogs => _habitLogs ??= new HabitLogRepository(_dbContext);
    public ITicketRepository Tickets => _tickets ??= new TicketRepository(_dbContext);
    public ITicketMessageRepository TicketMessages => _ticketMessages ??= new TicketMessageRepository(_dbContext);
    public IResumeRepository Resumes => _resumes ??= new ResumeRepository(_dbContext);
    public IDebtRepository Debts => _debts ??= new DebtRepository(_dbContext);
    public IExpenseRepository Expenses => _expenses ??= new ExpenseRepository(_dbContext);
    public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_dbContext);
    public IRolePermissionRepository RolePermissions => _rolePermissions ??= new RolePermissionRepository(_dbContext);
    public IUserPermissionRepository UserPermissions => _userPermissions ??= new UserPermissionRepository(_dbContext);
    public IPomodoroRepository Pomodoros => _pomodoros ??= new PomodoroRepository(_dbContext);

    // Football Fantasy
    private ICountryRepository? _countries;
    private IVenueRepository? _venues;
    private ILeagueRepository? _leagues;
    private ISeasonRepository? _seasons;
    private IStageRepository? _stages;
    private ITeamRepository? _teams;
    private ITeamSeasonRepository? _teamSeasons;
    private IPlayerRepository? _players;
    private IPlayerSquadRepository? _playerSquads;
    private ICoachRepository? _coaches;
    private ITeamCoachRepository? _teamCoaches;
    private IFixtureRepository? _fixtures;
    private IFixtureEventRepository? _fixtureEvents;
    private IPlayerMatchStatRepository? _playerMatchStats;
    private IPlayerPriceRepository? _playerPrices;
    private IScoringRuleSetRepository? _scoringRuleSets;
    private IScoringRuleRepository? _scoringRules;
    private IFantasyLeagueRepository? _fantasyLeagues;
    private IFantasyLeaguePositionLimitRepository? _fantasyLeaguePositionLimits;
    private IFantasyLeagueSourceRepository? _fantasyLeagueSources;
    private IFantasyLeaguePoolRuleRepository? _fantasyLeaguePoolRules;
    private IFantasyLeagueMemberRepository? _fantasyLeagueMembers;
    private IFantasyLeagueInviteRepository? _fantasyLeagueInvites;
    private IFantasyPeriodRepository? _fantasyPeriods;
    private IFantasyPeriodFixtureRepository? _fantasyPeriodFixtures;
    private IFantasyTeamRepository? _fantasyTeams;
    private IFantasySquadEntryRepository? _fantasySquadEntries;
    private IFantasyLineupRepository? _fantasyLineups;
    private IFantasyLineupSlotRepository? _fantasyLineupSlots;
    private IFantasyPlayerScoreRepository? _fantasyPlayerScores;
    private IFantasyTeamPeriodScoreRepository? _fantasyTeamPeriodScores;
    private IAchievementRepository? _achievements;
    private IUserAchievementRepository? _userAchievements;
    private IFantasyLeagueGroupRepository? _fantasyLeagueGroups;
    private IFantasyMatchupRepository? _fantasyMatchups;
    private IFantasyTeamChipRepository? _fantasyTeamChips;

    public ICountryRepository Countries => _countries ??= new CountryRepository(_dbContext);
    public IVenueRepository Venues => _venues ??= new VenueRepository(_dbContext);
    public ILeagueRepository Leagues => _leagues ??= new LeagueRepository(_dbContext);
    public ISeasonRepository Seasons => _seasons ??= new SeasonRepository(_dbContext);
    public IStageRepository Stages => _stages ??= new StageRepository(_dbContext);
    public ITeamRepository Teams => _teams ??= new TeamRepository(_dbContext);
    public ITeamSeasonRepository TeamSeasons => _teamSeasons ??= new TeamSeasonRepository(_dbContext);
    public IPlayerRepository Players => _players ??= new PlayerRepository(_dbContext);
    public IPlayerSquadRepository PlayerSquads => _playerSquads ??= new PlayerSquadRepository(_dbContext);
    public ICoachRepository Coaches => _coaches ??= new CoachRepository(_dbContext);
    public ITeamCoachRepository TeamCoaches => _teamCoaches ??= new TeamCoachRepository(_dbContext);
    public IFixtureRepository Fixtures => _fixtures ??= new FixtureRepository(_dbContext);
    public IFixtureEventRepository FixtureEvents => _fixtureEvents ??= new FixtureEventRepository(_dbContext);
    public IPlayerMatchStatRepository PlayerMatchStats => _playerMatchStats ??= new PlayerMatchStatRepository(_dbContext);
    public IPlayerPriceRepository PlayerPrices => _playerPrices ??= new PlayerPriceRepository(_dbContext);
    public IScoringRuleSetRepository ScoringRuleSets => _scoringRuleSets ??= new ScoringRuleSetRepository(_dbContext);
    public IScoringRuleRepository ScoringRules => _scoringRules ??= new ScoringRuleRepository(_dbContext);
    public IFantasyLeagueRepository FantasyLeagues => _fantasyLeagues ??= new FantasyLeagueRepository(_dbContext);
    public IFantasyLeaguePositionLimitRepository FantasyLeaguePositionLimits => _fantasyLeaguePositionLimits ??= new FantasyLeaguePositionLimitRepository(_dbContext);
    public IFantasyLeagueSourceRepository FantasyLeagueSources => _fantasyLeagueSources ??= new FantasyLeagueSourceRepository(_dbContext);
    public IFantasyLeaguePoolRuleRepository FantasyLeaguePoolRules => _fantasyLeaguePoolRules ??= new FantasyLeaguePoolRuleRepository(_dbContext);
    public IFantasyLeagueMemberRepository FantasyLeagueMembers => _fantasyLeagueMembers ??= new FantasyLeagueMemberRepository(_dbContext);
    public IFantasyLeagueInviteRepository FantasyLeagueInvites => _fantasyLeagueInvites ??= new FantasyLeagueInviteRepository(_dbContext);
    public IFantasyPeriodRepository FantasyPeriods => _fantasyPeriods ??= new FantasyPeriodRepository(_dbContext);
    public IFantasyPeriodFixtureRepository FantasyPeriodFixtures => _fantasyPeriodFixtures ??= new FantasyPeriodFixtureRepository(_dbContext);
    public IFantasyTeamRepository FantasyTeams => _fantasyTeams ??= new FantasyTeamRepository(_dbContext);
    public IFantasySquadEntryRepository FantasySquadEntries => _fantasySquadEntries ??= new FantasySquadEntryRepository(_dbContext);
    public IFantasyLineupRepository FantasyLineups => _fantasyLineups ??= new FantasyLineupRepository(_dbContext);
    public IFantasyLineupSlotRepository FantasyLineupSlots => _fantasyLineupSlots ??= new FantasyLineupSlotRepository(_dbContext);
    public IFantasyPlayerScoreRepository FantasyPlayerScores => _fantasyPlayerScores ??= new FantasyPlayerScoreRepository(_dbContext);
    public IFantasyTeamPeriodScoreRepository FantasyTeamPeriodScores => _fantasyTeamPeriodScores ??= new FantasyTeamPeriodScoreRepository(_dbContext);
    public IAchievementRepository Achievements => _achievements ??= new AchievementRepository(_dbContext);
    public IUserAchievementRepository UserAchievements => _userAchievements ??= new UserAchievementRepository(_dbContext);
    public IFantasyLeagueGroupRepository FantasyLeagueGroups => _fantasyLeagueGroups ??= new FantasyLeagueGroupRepository(_dbContext);
    public IFantasyMatchupRepository FantasyMatchups => _fantasyMatchups ??= new FantasyMatchupRepository(_dbContext);
    public IFantasyTeamChipRepository FantasyTeamChips => _fantasyTeamChips ??= new FantasyTeamChipRepository(_dbContext);

    // Predict Club
    private IMatchStatRepository? _matchStats;
    private IPredictClubRepository? _predictClubs;
    private IPredictClubMemberRepository? _predictClubMembers;
    private IMatchPredictionRepository? _matchPredictions;
    private ITournamentPredictionRepository? _tournamentPredictions;

    public IMatchStatRepository MatchStats => _matchStats ??= new MatchStatRepository(_dbContext);
    public IPredictClubRepository PredictClubs => _predictClubs ??= new PredictClubRepository(_dbContext);
    public IPredictClubMemberRepository PredictClubMembers => _predictClubMembers ??= new PredictClubMemberRepository(_dbContext);
    public IMatchPredictionRepository MatchPredictions => _matchPredictions ??= new MatchPredictionRepository(_dbContext);
    public ITournamentPredictionRepository TournamentPredictions => _tournamentPredictions ??= new TournamentPredictionRepository(_dbContext);

    // Extended API-Football Domains
    private ILeagueStandingRepository? _leagueStandings;
    private IBookmakerRepository? _bookmakers;
    private IFixtureOddRepository? _fixtureOdds;
    private IBetTypeRepository? _betTypes;
    private IPlayerTransferRepository? _playerTransfers;
    private IPlayerInjuryRepository? _playerInjuries;
    private ITeamTrophyRepository? _teamTrophies;
    private ISidelinedRepository? _sidelined;
    private IHeadToHeadRepository? _headToHeads;
    private ITeamStatisticRepository? _teamStatistics;
    private IFixtureLineupRepository? _fixtureLineups;
    private IFixtureStatisticRepository? _fixtureStatistics;
    private IFixtureLiveUpdateRepository? _fixtureLiveUpdates;
    private IPlayerTopScorerRepository? _playerTopScorers;
    private IPlayerSeasonStatisticRepository? _playerSeasonStatistics;
    private IPlayerProfileRepository? _playerProfiles;
    private IPlayerTeamHistoryRepository? _playerTeamHistories;

    public ILeagueStandingRepository LeagueStandings => _leagueStandings ??= new LeagueStandingRepository(_dbContext);
    public IBookmakerRepository Bookmakers => _bookmakers ??= new BookmakerRepository(_dbContext);
    public IFixtureOddRepository FixtureOdds => _fixtureOdds ??= new FixtureOddRepository(_dbContext);
    public IBetTypeRepository BetTypes => _betTypes ??= new BetTypeRepository(_dbContext);
    public IPlayerTransferRepository PlayerTransfers => _playerTransfers ??= new PlayerTransferRepository(_dbContext);
    public IPlayerInjuryRepository PlayerInjuries => _playerInjuries ??= new PlayerInjuryRepository(_dbContext);
    public ITeamTrophyRepository TeamTrophies => _teamTrophies ??= new TeamTrophyRepository(_dbContext);
    public ISidelinedRepository Sidelined => _sidelined ??= new SidelinedRepository(_dbContext);
    public IHeadToHeadRepository HeadToHeads => _headToHeads ??= new HeadToHeadRepository(_dbContext);
    public ITeamStatisticRepository TeamStatistics => _teamStatistics ??= new TeamStatisticRepository(_dbContext);
    public IFixtureLineupRepository FixtureLineups => _fixtureLineups ??= new FixtureLineupRepository(_dbContext);
    public IFixtureStatisticRepository FixtureStatistics => _fixtureStatistics ??= new FixtureStatisticRepository(_dbContext);
    public IFixtureLiveUpdateRepository FixtureLiveUpdates => _fixtureLiveUpdates ??= new FixtureLiveUpdateRepository(_dbContext);
    public IPlayerTopScorerRepository PlayerTopScorers => _playerTopScorers ??= new PlayerTopScorerRepository(_dbContext);
    public IPlayerSeasonStatisticRepository PlayerSeasonStatistics => _playerSeasonStatistics ??= new PlayerSeasonStatisticRepository(_dbContext);
    public IPlayerProfileRepository PlayerProfiles => _playerProfiles ??= new PlayerProfileRepository(_dbContext);
    public IPlayerTeamHistoryRepository PlayerTeamHistories => _playerTeamHistories ??= new PlayerTeamHistoryRepository(_dbContext);

    // Forex Trading & Market Analysis
    private AzureTest.Application.Interfaces.Data.Forex.IForexAccountRepository? _forexAccounts;
    private AzureTest.Application.Interfaces.Data.Forex.IForexOrderRepository? _forexOrders;
    private AzureTest.Application.Interfaces.Data.Forex.IForexTradeRepository? _forexTrades;
    private AzureTest.Application.Interfaces.Data.Forex.IForexStrategyRepository? _forexStrategies;
    private AzureTest.Application.Interfaces.Data.Forex.IForexBacktestRepository? _forexBacktests;
    private AzureTest.Application.Interfaces.Data.Forex.IForexEconomicAlertRepository? _forexEconomicAlerts;
    private AzureTest.Application.Interfaces.Data.Forex.IForexJournalEntryRepository? _forexJournalEntries;
    private AzureTest.Application.Interfaces.Data.Forex.IForexApiSettingRepository? _forexApiSettings;

    public AzureTest.Application.Interfaces.Data.Forex.IForexAccountRepository ForexAccounts => _forexAccounts ??= new Forex.ForexAccountRepository(_dbContext);
    public AzureTest.Application.Interfaces.Data.Forex.IForexOrderRepository ForexOrders => _forexOrders ??= new Forex.ForexOrderRepository(_dbContext);
    public AzureTest.Application.Interfaces.Data.Forex.IForexTradeRepository ForexTrades => _forexTrades ??= new Forex.ForexTradeRepository(_dbContext);
    public AzureTest.Application.Interfaces.Data.Forex.IForexStrategyRepository ForexStrategies => _forexStrategies ??= new Forex.ForexStrategyRepository(_dbContext);
    public AzureTest.Application.Interfaces.Data.Forex.IForexBacktestRepository ForexBacktests => _forexBacktests ??= new Forex.ForexBacktestRepository(_dbContext);
    public AzureTest.Application.Interfaces.Data.Forex.IForexEconomicAlertRepository ForexEconomicAlerts => _forexEconomicAlerts ??= new Forex.ForexEconomicAlertRepository(_dbContext);
    public AzureTest.Application.Interfaces.Data.Forex.IForexJournalEntryRepository ForexJournalEntries => _forexJournalEntries ??= new Forex.ForexJournalEntryRepository(_dbContext);
    public AzureTest.Application.Interfaces.Data.Forex.IForexApiSettingRepository ForexApiSettings => _forexApiSettings ??= new Forex.ForexApiSettingRepository(_dbContext);

    // Crypto Trading & Market Analysis
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoAccountRepository? _cryptoAccounts;
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoOrderRepository? _cryptoOrders;
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoPositionRepository? _cryptoPositions;
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoStrategyRepository? _cryptoStrategies;
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoBacktestRepository? _cryptoBacktests;
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoAlertRepository? _cryptoAlerts;
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoJournalEntryRepository? _cryptoJournalEntries;
    private AzureTest.Application.Interfaces.Data.Crypto.ICryptoApiSettingRepository? _cryptoApiSettings;

    private AzureTest.Application.Interfaces.Data.Trading.ITradingAccountRepository? _tradingAccounts;
    private AzureTest.Application.Interfaces.Data.Trading.ITradingPositionRepository? _tradingPositions;
    private AzureTest.Application.Interfaces.Data.Trading.ITradingOrderRepository? _tradingOrders;
    private AzureTest.Application.Interfaces.Data.Trading.ITradingWatchlistRepository? _tradingWatchlists;
    private AzureTest.Application.Interfaces.Data.Outbox.IOutboxRepository? _outboxMessages;
    private AzureTest.Application.Interfaces.Data.Outbox.IInboxRepository? _inboxMessages;

    public ICryptoAccountRepository CryptoAccounts => _cryptoAccounts ??= new Crypto.CryptoAccountRepository(_dbContext);
    public ICryptoOrderRepository CryptoOrders => _cryptoOrders ??= new Crypto.CryptoOrderRepository(_dbContext);
    public ICryptoPositionRepository CryptoPositions => _cryptoPositions ??= new Crypto.CryptoPositionRepository(_dbContext);
    public ICryptoStrategyRepository CryptoStrategies => _cryptoStrategies ??= new Crypto.CryptoStrategyRepository(_dbContext);
    public ICryptoBacktestRepository CryptoBacktests => _cryptoBacktests ??= new Crypto.CryptoBacktestRepository(_dbContext);
    public ICryptoAlertRepository CryptoAlerts => _cryptoAlerts ??= new Crypto.CryptoAlertRepository(_dbContext);
    public ICryptoJournalEntryRepository CryptoJournalEntries => _cryptoJournalEntries ??= new Crypto.CryptoJournalEntryRepository(_dbContext);
    public ICryptoApiSettingRepository CryptoApiSettings => _cryptoApiSettings ??= new Crypto.CryptoApiSettingRepository(_dbContext);

    public ITradingAccountRepository TradingAccounts => _tradingAccounts ??= new Trading.TradingAccountRepository(_dbContext);
    public ITradingPositionRepository TradingPositions => _tradingPositions ??= new Trading.TradingPositionRepository(_dbContext);
    public ITradingOrderRepository TradingOrders => _tradingOrders ??= new Trading.TradingOrderRepository(_dbContext);
    public ITradingWatchlistRepository TradingWatchlists => _tradingWatchlists ??= new Trading.TradingWatchlistRepository(_dbContext);

    public IOutboxRepository OutboxMessages => _outboxMessages ??= new Outbox.OutboxRepository(_dbContext);
    public IInboxRepository InboxMessages => _inboxMessages ??= new Outbox.InboxRepository(_dbContext);

    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _currentTransaction;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null && Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.IsRelational(_dbContext.Database))
        {
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    /// <summary>
    /// Returns domain events collected by the DomainEventsInterceptor after the last SaveChanges.
    /// Call this after SaveChangesAsync to dispatch events.
    /// </summary>
    public IReadOnlyList<IDomainEvent> PopDomainEvents()
    {
        return _dbContext.DomainEventsInterceptor.PopDomainEvents();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}

