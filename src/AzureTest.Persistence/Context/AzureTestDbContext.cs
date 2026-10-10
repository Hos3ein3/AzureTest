

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

namespace AzureTest.Persistence.Context;

public class AzureTestDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly DomainEventsInterceptor? _domainEventsInterceptor;

    public AzureTestDbContext(
        DbContextOptions<AzureTestDbContext> options,
        DomainEventsInterceptor? domainEventsInterceptor = null) : base(options)
    {
        _domainEventsInterceptor = domainEventsInterceptor;
    }

    public AzureTestDbContext(
        DbContextOptions<AzureTestDbContext> options) : base(options)
    {
    }

    protected AzureTestDbContext()
    {
    }

    /// <summary>Exposes collected domain events after SaveChanges for dispatch.</summary>
    public DomainEventsInterceptor? DomainEventsInterceptor => _domainEventsInterceptor;

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

    // Football Fantasy - Real-World Tables
    public DbSet<AzureTest.Domain.Entities.FootballFantasy.Country> Countries => Set<AzureTest.Domain.Entities.FootballFantasy.Country>();
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Apply a global query filter for all ISoftDelete entities.
        // Automatically excludes soft-deleted records from every query.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                continue;

            var param = Expression.Parameter(entityType.ClrType, "e");
            var prop = Expression.Property(param, nameof(ISoftDelete.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(prop), param);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }
}

