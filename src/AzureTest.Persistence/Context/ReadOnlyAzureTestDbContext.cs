using System;
using System.Linq.Expressions;
using System.Reflection;
using AzureTest.Domain.Entities;
using AzureTest.Domain.Entities.FootballFantasy;
using AzureTest.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace AzureTest.Persistence.Context;

/// <summary>
/// Read-Only DbContext dedicated to serving read queries across Football Fantasy and Prediction modules.
/// Configured with QueryTrackingBehavior.NoTracking by default to maximize throughput on read replicas.
/// </summary>
public class ReadOnlyAzureTestDbContext : DbContext
{
    public ReadOnlyAzureTestDbContext(DbContextOptions<ReadOnlyAzureTestDbContext> options)
        : base(options)
    {
    }

    // Football Fantasy Core
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Stage> Stages => Set<Stage>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamSeason> TeamSeasons => Set<TeamSeason>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<PlayerSquad> PlayerSquads => Set<PlayerSquad>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<TeamCoach> TeamCoaches => Set<TeamCoach>();
    public DbSet<Fixture> Fixtures => Set<Fixture>();
    public DbSet<FixtureEvent> FixtureEvents => Set<FixtureEvent>();
    public DbSet<PlayerMatchStat> PlayerMatchStats => Set<PlayerMatchStat>();
    public DbSet<PlayerPrice> PlayerPrices => Set<PlayerPrice>();

    // Scoring & Leagues
    public DbSet<ScoringRuleSet> ScoringRuleSets => Set<ScoringRuleSet>();
    public DbSet<ScoringRule> ScoringRules => Set<ScoringRule>();
    public DbSet<FantasyLeague> FantasyLeagues => Set<FantasyLeague>();
    public DbSet<FantasyLeaguePositionLimit> FantasyLeaguePositionLimits => Set<FantasyLeaguePositionLimit>();
    public DbSet<FantasyLeagueSource> FantasyLeagueSources => Set<FantasyLeagueSource>();
    public DbSet<FantasyLeaguePoolRule> FantasyLeaguePoolRules => Set<FantasyLeaguePoolRule>();
    public DbSet<FantasyLeagueMember> FantasyLeagueMembers => Set<FantasyLeagueMember>();
    public DbSet<FantasyLeagueInvite> FantasyLeagueInvites => Set<FantasyLeagueInvite>();
    public DbSet<FantasyPeriod> FantasyPeriods => Set<FantasyPeriod>();
    public DbSet<FantasyPeriodFixture> FantasyPeriodFixtures => Set<FantasyPeriodFixture>();
    public DbSet<FantasyTeam> FantasyTeams => Set<FantasyTeam>();
    public DbSet<FantasySquadEntry> FantasySquadEntries => Set<FantasySquadEntry>();
    public DbSet<FantasyLineup> FantasyLineups => Set<FantasyLineup>();
    public DbSet<FantasyLineupSlot> FantasyLineupSlots => Set<FantasyLineupSlot>();
    public DbSet<FantasyPlayerScore> FantasyPlayerScores => Set<FantasyPlayerScore>();
    public DbSet<FantasyTeamPeriodScore> FantasyTeamPeriodScores => Set<FantasyTeamPeriodScore>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<FantasyLeagueGroup> FantasyLeagueGroups => Set<FantasyLeagueGroup>();
    public DbSet<FantasyMatchup> FantasyMatchups => Set<FantasyMatchup>();
    public DbSet<FantasyTeamChip> FantasyTeamChips => Set<FantasyTeamChip>();

    // Predict Club & Match Stats
    public DbSet<MatchStat> MatchStats => Set<MatchStat>();
    public DbSet<PredictClub> PredictClubs => Set<PredictClub>();
    public DbSet<PredictClubMember> PredictClubMembers => Set<PredictClubMember>();
    public DbSet<MatchPrediction> MatchPredictions => Set<MatchPrediction>();
    public DbSet<TournamentPrediction> TournamentPredictions => Set<TournamentPrediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Soft delete global filter
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

    public override int SaveChanges()
    {
        throw new InvalidOperationException("ReadOnlyAzureTestDbContext is read-only. Cannot save changes.");
    }

    public override System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("ReadOnlyAzureTestDbContext is read-only. Cannot save changes.");
    }
}
