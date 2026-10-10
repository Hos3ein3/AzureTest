using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerMatchStat : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const decimal MinProviderRating = 0.0m;
        public const decimal MaxProviderRating = 10.0m;
        public const int RatingPrecision = 4;
        public const int RatingScale = 2;
    }

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;
    public bool IsStarter { get; private set; }
    public int MinutesPlayed { get; private set; }
    public int Goals { get; private set; }
    public int Assists { get; private set; }
    public int OwnGoals { get; private set; }
    public int ShotsTotal { get; private set; }
    public int ShotsOnTarget { get; private set; }
    public int PassesTotal { get; private set; }
    public int KeyPasses { get; private set; }
    public int Tackles { get; private set; }
    public int Interceptions { get; private set; }
    public int Saves { get; private set; }
    public int GoalsConceded { get; private set; }
    public int PenaltiesSaved { get; private set; }
    public int PenaltiesMissed { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }
    public bool CleanSheet { get; private set; }
    public decimal? ProviderRating { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerMatchStat() { }

    public static PlayerMatchStat Create(
        Guid fixtureId,
        Guid playerId,
        Guid teamId,
        bool isStarter,
        int minutesPlayed,
        int goals = 0,
        int assists = 0,
        int ownGoals = 0,
        int shotsTotal = 0,
        int shotsOnTarget = 0,
        int passesTotal = 0,
        int keyPasses = 0,
        int tackles = 0,
        int interceptions = 0,
        int saves = 0,
        int goalsConceded = 0,
        int penaltiesSaved = 0,
        int penaltiesMissed = 0,
        int yellowCards = 0,
        int redCards = 0,
        bool cleanSheet = false,
        decimal? providerRating = null)
    {
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));

        return new PlayerMatchStat
        {
            FixtureId = fixtureId,
            PlayerId = playerId,
            TeamId = teamId,
            IsStarter = isStarter,
            MinutesPlayed = minutesPlayed,
            Goals = goals,
            Assists = assists,
            OwnGoals = ownGoals,
            ShotsTotal = shotsTotal,
            ShotsOnTarget = shotsOnTarget,
            PassesTotal = passesTotal,
            KeyPasses = keyPasses,
            Tackles = tackles,
            Interceptions = interceptions,
            Saves = saves,
            GoalsConceded = goalsConceded,
            PenaltiesSaved = penaltiesSaved,
            PenaltiesMissed = penaltiesMissed,
            YellowCards = yellowCards,
            RedCards = redCards,
            CleanSheet = cleanSheet,
            ProviderRating = providerRating
        };
    }

    public void UpdateStats(
        bool isStarter,
        int minutesPlayed,
        int goals,
        int assists,
        int ownGoals,
        int shotsTotal,
        int shotsOnTarget,
        int passesTotal,
        int keyPasses,
        int tackles,
        int interceptions,
        int saves,
        int goalsConceded,
        int penaltiesSaved,
        int penaltiesMissed,
        int yellowCards,
        int redCards,
        bool cleanSheet,
        decimal? providerRating)
    {
        IsStarter = isStarter;
        MinutesPlayed = minutesPlayed;
        Goals = goals;
        Assists = assists;
        OwnGoals = ownGoals;
        ShotsTotal = shotsTotal;
        ShotsOnTarget = shotsOnTarget;
        PassesTotal = passesTotal;
        KeyPasses = keyPasses;
        Tackles = tackles;
        Interceptions = interceptions;
        Saves = saves;
        GoalsConceded = goalsConceded;
        PenaltiesSaved = penaltiesSaved;
        PenaltiesMissed = penaltiesMissed;
        YellowCards = yellowCards;
        RedCards = redCards;
        CleanSheet = cleanSheet;
        ProviderRating = providerRating;
    }
}
