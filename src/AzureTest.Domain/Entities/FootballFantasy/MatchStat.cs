using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

/// <summary>
/// Real-world aggregated match statistics for a fixture (possession, shots, corners, cards, etc.)
/// </summary>
public sealed class MatchStat : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int MinStatValue = 0;
        public const int MaxPercentage = 100;
    }

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;

    public Guid? MvpPlayerId { get; private set; }
    public Player? MvpPlayer { get; private set; }

    // Possession & Shots
    public int HomePossession { get; private set; }
    public int AwayPossession { get; private set; }
    public int HomeShotsTotal { get; private set; }
    public int AwayShotsTotal { get; private set; }
    public int HomeShotsOnTarget { get; private set; }
    public int AwayShotsOnTarget { get; private set; }

    // Corners, Fouls, Cards, Offsides
    public int HomeCorners { get; private set; }
    public int AwayCorners { get; private set; }
    public int HomeFouls { get; private set; }
    public int AwayFouls { get; private set; }
    public int HomeYellowCards { get; private set; }
    public int AwayYellowCards { get; private set; }
    public int HomeRedCards { get; private set; }
    public int AwayRedCards { get; private set; }
    public int HomeOffsides { get; private set; }
    public int AwayOffsides { get; private set; }

    // First Half result
    public int? HomeHalfTimeScore { get; private set; }
    public int? AwayHalfTimeScore { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private MatchStat() { }

    public static MatchStat Create(
        Guid fixtureId,
        int homePossession = 50,
        int awayPossession = 50,
        int homeShotsTotal = 0,
        int awayShotsTotal = 0,
        int homeShotsOnTarget = 0,
        int awayShotsOnTarget = 0,
        int homeCorners = 0,
        int awayCorners = 0,
        int homeFouls = 0,
        int awayFouls = 0,
        int homeYellowCards = 0,
        int awayYellowCards = 0,
        int homeRedCards = 0,
        int awayRedCards = 0,
        int homeOffsides = 0,
        int awayOffsides = 0,
        int? homeHalfTimeScore = null,
        int? awayHalfTimeScore = null,
        Guid? mvpPlayerId = null)
    {
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));

        return new MatchStat
        {
            FixtureId = fixtureId,
            HomePossession = Math.Clamp(homePossession, Rules.MinStatValue, Rules.MaxPercentage),
            AwayPossession = Math.Clamp(awayPossession, Rules.MinStatValue, Rules.MaxPercentage),
            HomeShotsTotal = Math.Max(Rules.MinStatValue, homeShotsTotal),
            AwayShotsTotal = Math.Max(Rules.MinStatValue, awayShotsTotal),
            HomeShotsOnTarget = Math.Max(Rules.MinStatValue, homeShotsOnTarget),
            AwayShotsOnTarget = Math.Max(Rules.MinStatValue, awayShotsOnTarget),
            HomeCorners = Math.Max(Rules.MinStatValue, homeCorners),
            AwayCorners = Math.Max(Rules.MinStatValue, awayCorners),
            HomeFouls = Math.Max(Rules.MinStatValue, homeFouls),
            AwayFouls = Math.Max(Rules.MinStatValue, awayFouls),
            HomeYellowCards = Math.Max(Rules.MinStatValue, homeYellowCards),
            AwayYellowCards = Math.Max(Rules.MinStatValue, awayYellowCards),
            HomeRedCards = Math.Max(Rules.MinStatValue, homeRedCards),
            AwayRedCards = Math.Max(Rules.MinStatValue, awayRedCards),
            HomeOffsides = Math.Max(Rules.MinStatValue, homeOffsides),
            AwayOffsides = Math.Max(Rules.MinStatValue, awayOffsides),
            HomeHalfTimeScore = homeHalfTimeScore,
            AwayHalfTimeScore = awayHalfTimeScore,
            MvpPlayerId = mvpPlayerId
        };
    }

    public void UpdateStats(
        int homePossession,
        int awayPossession,
        int homeShotsTotal,
        int awayShotsTotal,
        int homeShotsOnTarget,
        int awayShotsOnTarget,
        int homeCorners,
        int awayCorners,
        int homeFouls,
        int awayFouls,
        int homeYellowCards,
        int awayYellowCards,
        int homeRedCards,
        int awayRedCards,
        int homeOffsides,
        int awayOffsides,
        int? homeHalfTimeScore = null,
        int? awayHalfTimeScore = null,
        Guid? mvpPlayerId = null)
    {
        HomePossession = Math.Clamp(homePossession, Rules.MinStatValue, Rules.MaxPercentage);
        AwayPossession = Math.Clamp(awayPossession, Rules.MinStatValue, Rules.MaxPercentage);
        HomeShotsTotal = Math.Max(Rules.MinStatValue, homeShotsTotal);
        AwayShotsTotal = Math.Max(Rules.MinStatValue, awayShotsTotal);
        HomeShotsOnTarget = Math.Max(Rules.MinStatValue, homeShotsOnTarget);
        AwayShotsOnTarget = Math.Max(Rules.MinStatValue, awayShotsOnTarget);
        HomeCorners = Math.Max(Rules.MinStatValue, homeCorners);
        AwayCorners = Math.Max(Rules.MinStatValue, awayCorners);
        HomeFouls = Math.Max(Rules.MinStatValue, homeFouls);
        AwayFouls = Math.Max(Rules.MinStatValue, awayFouls);
        HomeYellowCards = Math.Max(Rules.MinStatValue, homeYellowCards);
        AwayYellowCards = Math.Max(Rules.MinStatValue, awayYellowCards);
        HomeRedCards = Math.Max(Rules.MinStatValue, homeRedCards);
        AwayRedCards = Math.Max(Rules.MinStatValue, awayRedCards);
        HomeOffsides = Math.Max(Rules.MinStatValue, homeOffsides);
        AwayOffsides = Math.Max(Rules.MinStatValue, awayOffsides);
        HomeHalfTimeScore = homeHalfTimeScore;
        AwayHalfTimeScore = awayHalfTimeScore;
        MvpPlayerId = mvpPlayerId;
    }
}
