

using AzureTest.Application.Interfaces.Services;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Entities.Identity;
using AzureTest.Domain.Primitives;
using AzureTest.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AzureTest.Persistence.Context;

public class AzureTestDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly DomainEventsInterceptor? _domainEventsInterceptor;
    private readonly ITenantProvider? _tenantProvider;

    // public AzureTestDbContext(
    //     DbContextOptions<AzureTestDbContext> options,
    //     DomainEventsInterceptor? domainEventsInterceptor = null,
    //     ITenantProvider? tenantProvider = null) : base(options)
    // {
    //     _domainEventsInterceptor = domainEventsInterceptor;
    //     _tenantProvider = tenantProvider;
    // }

    public AzureTestDbContext(
        DbContextOptions<AzureTestDbContext> options,
        ITenantProvider? tenantProvider = null) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    protected AzureTestDbContext()
    {
    }

    /// <summary>Exposes collected domain events after SaveChanges for dispatch.</summary>
    public DomainEventsInterceptor? DomainEventsInterceptor => _domainEventsInterceptor;

    /// <summary>Active tenant ID for query filter evaluation.</summary>
    public virtual Guid CurrentTenantId => _tenantProvider?.IsMultiTenancyEnabled == true
        ? _tenantProvider.CurrentTenantId
        : Guid.Empty;

    /// <summary>Indicates whether multi-tenancy is active via configuration feature flags.</summary>
    public virtual bool IsMultiTenancyEnabled => _tenantProvider?.IsMultiTenancyEnabled == true;

    public DbSet<Resume> Resumes => Set<Resume>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    // New Feature Domains
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<HabitTracker> HabitTrackers => Set<HabitTracker>();
    public DbSet<HabitLog> HabitLogs => Set<HabitLog>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<Debt> Debts => Set<Debt>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<AzureTest.Domain.Entities.Pomodoro.PomodoroSession> PomodoroSessions => Set<AzureTest.Domain.Entities.Pomodoro.PomodoroSession>();
    public DbSet<AzureTest.Domain.Entities.Pomodoro.PomodoroSetting> PomodoroSettings => Set<AzureTest.Domain.Entities.Pomodoro.PomodoroSetting>();

    // Football Fantasy - Real-World Tables
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Country> Countries => Set<AzureTest.Domain.Entities.FootballFantasy.Country>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Venue> Venues => Set<AzureTest.Domain.Entities.FootballFantasy.Venue>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.League> Leagues => Set<AzureTest.Domain.Entities.FootballFantasy.League>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Season> Seasons => Set<AzureTest.Domain.Entities.FootballFantasy.Season>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Stage> Stages => Set<AzureTest.Domain.Entities.FootballFantasy.Stage>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Team> Teams => Set<AzureTest.Domain.Entities.FootballFantasy.Team>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.TeamSeason> TeamSeasons => Set<AzureTest.Domain.Entities.FootballFantasy.TeamSeason>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Player> Players => Set<AzureTest.Domain.Entities.FootballFantasy.Player>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerSquad> PlayerSquads => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerSquad>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Coach> Coaches => Set<AzureTest.Domain.Entities.FootballFantasy.Coach>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.TeamCoach> TeamCoaches => Set<AzureTest.Domain.Entities.FootballFantasy.TeamCoach>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Fixture> Fixtures => Set<AzureTest.Domain.Entities.FootballFantasy.Fixture>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FixtureEvent> FixtureEvents => Set<AzureTest.Domain.Entities.FootballFantasy.FixtureEvent>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerMatchStat> PlayerMatchStats => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerMatchStat>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerPrice> PlayerPrices => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerPrice>();

    // Extended Football API Domains
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.LeagueStanding> LeagueStandings => Set<AzureTest.Domain.Entities.FootballFantasy.LeagueStanding>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Bookmaker> Bookmakers => Set<AzureTest.Domain.Entities.FootballFantasy.Bookmaker>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FixtureOdd> FixtureOdds => Set<AzureTest.Domain.Entities.FootballFantasy.FixtureOdd>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.BetType> BetTypes => Set<AzureTest.Domain.Entities.FootballFantasy.BetType>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerTransfer> PlayerTransfers => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerTransfer>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerInjury> PlayerInjuries => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerInjury>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.TeamTrophy> TeamTrophies => Set<AzureTest.Domain.Entities.FootballFantasy.TeamTrophy>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Sidelined> Sidelined => Set<AzureTest.Domain.Entities.FootballFantasy.Sidelined>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.HeadToHead> HeadToHeads => Set<AzureTest.Domain.Entities.FootballFantasy.HeadToHead>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.TeamStatistic> TeamStatistics => Set<AzureTest.Domain.Entities.FootballFantasy.TeamStatistic>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FixtureLineup> FixtureLineups => Set<AzureTest.Domain.Entities.FootballFantasy.FixtureLineup>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FixtureStatistic> FixtureStatistics => Set<AzureTest.Domain.Entities.FootballFantasy.FixtureStatistic>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FixtureLiveUpdate> FixtureLiveUpdates => Set<AzureTest.Domain.Entities.FootballFantasy.FixtureLiveUpdate>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerTopScorer> PlayerTopScorers => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerTopScorer>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerSeasonStatistic> PlayerSeasonStatistics => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerSeasonStatistic>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerProfile> PlayerProfiles => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerProfile>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PlayerTeamHistory> PlayerTeamHistories => Set<AzureTest.Domain.Entities.FootballFantasy.PlayerTeamHistory>();

    // Football Fantasy - Game Tables
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.ScoringRuleSet> ScoringRuleSets => Set<AzureTest.Domain.Entities.FootballFantasy.ScoringRuleSet>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.ScoringRule> ScoringRules => Set<AzureTest.Domain.Entities.FootballFantasy.ScoringRule>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLeague> FantasyLeagues => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLeague>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLeaguePositionLimit> FantasyLeaguePositionLimits => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLeaguePositionLimit>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueSource> FantasyLeagueSources => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueSource>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLeaguePoolRule> FantasyLeaguePoolRules => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLeaguePoolRule>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueMember> FantasyLeagueMembers => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueMember>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueInvite> FantasyLeagueInvites => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueInvite>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyPeriod> FantasyPeriods => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyPeriod>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyPeriodFixture> FantasyPeriodFixtures => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyPeriodFixture>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyTeam> FantasyTeams => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyTeam>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasySquadEntry> FantasySquadEntries => Set<AzureTest.Domain.Entities.FootballFantasy.FantasySquadEntry>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLineup> FantasyLineups => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLineup>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLineupSlot> FantasyLineupSlots => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLineupSlot>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyPlayerScore> FantasyPlayerScores => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyPlayerScore>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyTeamPeriodScore> FantasyTeamPeriodScores => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyTeamPeriodScore>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Achievement> Achievements => Set<AzureTest.Domain.Entities.FootballFantasy.Achievement>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.UserAchievement> UserAchievements => Set<AzureTest.Domain.Entities.FootballFantasy.UserAchievement>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueGroup> FantasyLeagueGroups => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyLeagueGroup>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyMatchup> FantasyMatchups => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyMatchup>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.FantasyTeamChip> FantasyTeamChips => Set<AzureTest.Domain.Entities.FootballFantasy.FantasyTeamChip>();

    // Predict Club & Match Stats
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.MatchStat> MatchStats => Set<AzureTest.Domain.Entities.FootballFantasy.MatchStat>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PredictClub> PredictClubs => Set<AzureTest.Domain.Entities.FootballFantasy.PredictClub>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.PredictClubMember> PredictClubMembers => Set<AzureTest.Domain.Entities.FootballFantasy.PredictClubMember>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.MatchPrediction> MatchPredictions => Set<AzureTest.Domain.Entities.FootballFantasy.MatchPrediction>();
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.TournamentPrediction> TournamentPredictions => Set<AzureTest.Domain.Entities.FootballFantasy.TournamentPrediction>();

    // Forex Trading & Market Analysis
    public DbSet<AzureTest.Domain.Entities.Forex.ForexAccount> ForexAccounts => Set<AzureTest.Domain.Entities.Forex.ForexAccount>();
    public DbSet<AzureTest.Domain.Entities.Forex.ForexOrder> ForexOrders => Set<AzureTest.Domain.Entities.Forex.ForexOrder>();
    public DbSet<AzureTest.Domain.Entities.Forex.ForexTrade> ForexTrades => Set<AzureTest.Domain.Entities.Forex.ForexTrade>();
    public DbSet<AzureTest.Domain.Entities.Forex.ForexStrategy> ForexStrategies => Set<AzureTest.Domain.Entities.Forex.ForexStrategy>();
    public DbSet<AzureTest.Domain.Entities.Forex.ForexBacktest> ForexBacktests => Set<AzureTest.Domain.Entities.Forex.ForexBacktest>();
    public DbSet<AzureTest.Domain.Entities.Forex.ForexEconomicAlert> ForexEconomicAlerts => Set<AzureTest.Domain.Entities.Forex.ForexEconomicAlert>();
    public DbSet<AzureTest.Domain.Entities.Forex.ForexJournalEntry> ForexJournalEntries => Set<AzureTest.Domain.Entities.Forex.ForexJournalEntry>();
    public DbSet<AzureTest.Domain.Entities.Forex.ForexApiSetting> ForexApiSettings => Set<AzureTest.Domain.Entities.Forex.ForexApiSetting>();

    // Crypto Trading & Market Analysis
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoAccount> CryptoAccounts => Set<AzureTest.Domain.Entities.Crypto.CryptoAccount>();
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoOrder> CryptoOrders => Set<AzureTest.Domain.Entities.Crypto.CryptoOrder>();
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoPosition> CryptoPositions => Set<AzureTest.Domain.Entities.Crypto.CryptoPosition>();
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoStrategy> CryptoStrategies => Set<AzureTest.Domain.Entities.Crypto.CryptoStrategy>();
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoBacktest> CryptoBacktests => Set<AzureTest.Domain.Entities.Crypto.CryptoBacktest>();
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoAlert> CryptoAlerts => Set<AzureTest.Domain.Entities.Crypto.CryptoAlert>();
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoJournalEntry> CryptoJournalEntries => Set<AzureTest.Domain.Entities.Crypto.CryptoJournalEntry>();
    public DbSet<AzureTest.Domain.Entities.Crypto.CryptoApiSetting> CryptoApiSettings => Set<AzureTest.Domain.Entities.Crypto.CryptoApiSetting>();

    // Universal Paper Trading
    public DbSet<AzureTest.Domain.Entities.Trading.TradingAccount> TradingAccounts => Set<AzureTest.Domain.Entities.Trading.TradingAccount>();
    public DbSet<AzureTest.Domain.Entities.Trading.TradingPosition> TradingPositions => Set<AzureTest.Domain.Entities.Trading.TradingPosition>();
    public DbSet<AzureTest.Domain.Entities.Trading.TradingOrder> TradingOrders => Set<AzureTest.Domain.Entities.Trading.TradingOrder>();
    public DbSet<AzureTest.Domain.Entities.Trading.TradingWatchlist> TradingWatchlists => Set<AzureTest.Domain.Entities.Trading.TradingWatchlist>();

    // Event-Driven Outbox & Inbox
    public DbSet<AzureTest.Domain.Entities.Outbox.OutboxMessage> OutboxMessages => Set<AzureTest.Domain.Entities.Outbox.OutboxMessage>();
    public DbSet<AzureTest.Domain.Entities.Outbox.InboxMessage> InboxMessages => Set<AzureTest.Domain.Entities.Outbox.InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply a unified global query filter for ISoftDelete and ITenantEntity.
        // Combining filters into a single lambda avoids EF Core's HasQueryFilter overwrite limitation.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            bool isSoftDelete = typeof(ISoftDelete).IsAssignableFrom(clrType);
            bool isTenantEntity = typeof(ITenantEntity).IsAssignableFrom(clrType);

            if (!isSoftDelete && !isTenantEntity)
                continue;

            var param = Expression.Parameter(clrType, "e");
            Expression? combinedExpr = null;

            if (isSoftDelete)
            {
                var isDeletedProp = Expression.Property(param, nameof(ISoftDelete.IsDeleted));
                combinedExpr = Expression.Not(isDeletedProp); // !e.IsDeleted
            }

            if (isTenantEntity)
            {
                // Multi-tenancy filter expression:
                // (!this.IsMultiTenancyEnabled || this.CurrentTenantId == Guid.Empty || e.TenantId == this.CurrentTenantId)
                var thisExpr = Expression.Constant(this);

                var isEnabledProp = Expression.Property(thisExpr, nameof(IsMultiTenancyEnabled));
                var disabledExpr = Expression.Not(isEnabledProp);

                var currentTenantProp = Expression.Property(thisExpr, nameof(CurrentTenantId));
                var emptyTenantExpr = Expression.Equal(currentTenantProp, Expression.Constant(Guid.Empty));

                var entityTenantProp = Expression.Property(param, nameof(ITenantEntity.TenantId));
                var tenantMatchExpr = Expression.Equal(entityTenantProp, currentTenantProp);

                var tenantFilterExpr = Expression.OrElse(
                    disabledExpr,
                    Expression.OrElse(emptyTenantExpr, tenantMatchExpr)
                );

                combinedExpr = combinedExpr == null
                    ? tenantFilterExpr
                    : Expression.AndAlso(combinedExpr, tenantFilterExpr);
            }

            if (combinedExpr != null)
            {
                var lambda = Expression.Lambda(combinedExpr, param);
                modelBuilder.Entity(clrType).HasQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTenantId();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTenantId();
        return base.SaveChanges();
    }

    private void ApplyTenantId()
    {
        if (IsMultiTenancyEnabled && CurrentTenantId != Guid.Empty)
        {
            foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
            {
                if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
                {
                    entry.Entity.TenantId = CurrentTenantId;
                }
            }
        }
    }
}


