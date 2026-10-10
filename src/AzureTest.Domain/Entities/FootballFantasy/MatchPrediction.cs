using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

/// <summary>
/// User's prediction for an upcoming fixture in a Predict Club.
/// Can be edited as draft up to 1 minute before kickoff, when it automatically finalizes/locks.
/// </summary>
public sealed class MatchPrediction : BaseEntity, ISoftDelete, IAudit
{
    public Guid PredictClubId { get; private set; }
    public PredictClub PredictClub { get; private set; } = default!;

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;

    public Guid UserId { get; private set; }
    public PredictionEntryStatus Status { get; private set; } = PredictionEntryStatus.Draft;

    // Match Predictions
    public int? PredictedHomeScore { get; private set; }
    public int? PredictedAwayScore { get; private set; }
    public int? PredictedHalfTimeHomeScore { get; private set; }
    public int? PredictedHalfTimeAwayScore { get; private set; }
    public bool? BothTeamsToScore { get; private set; }
    public decimal? TotalGoalsOverUnder { get; private set; }
    public int? PredictedTotalYellowCards { get; private set; }
    public int? PredictedTotalCorners { get; private set; }
    public int? PredictedFirstGoalMinute { get; private set; }
    public Guid? PredictedMvpPlayerId { get; private set; }
    public Player? PredictedMvpPlayer { get; private set; }

    // Scoring result
    public int PointsEarned { get; private set; }
    public bool IsScored { get; private set; }
    public DateTime? ScoredAt { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private MatchPrediction() { }

    public static MatchPrediction Create(
        Guid predictClubId,
        Guid fixtureId,
        Guid userId,
        int? homeScore = null,
        int? awayScore = null,
        int? halfTimeHomeScore = null,
        int? halfTimeAwayScore = null,
        bool? btts = null,
        decimal? overUnder = null,
        int? cards = null,
        int? corners = null,
        int? firstGoalMinute = null,
        Guid? mvpPlayerId = null,
        bool isDraft = true)
    {
        if (predictClubId == Guid.Empty) throw new ArgumentException("PredictClubId is required.", nameof(predictClubId));
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));

        return new MatchPrediction
        {
            PredictClubId = predictClubId,
            FixtureId = fixtureId,
            UserId = userId,
            PredictedHomeScore = homeScore,
            PredictedAwayScore = awayScore,
            PredictedHalfTimeHomeScore = halfTimeHomeScore,
            PredictedHalfTimeAwayScore = halfTimeAwayScore,
            BothTeamsToScore = btts,
            TotalGoalsOverUnder = overUnder,
            PredictedTotalYellowCards = cards,
            PredictedTotalCorners = corners,
            PredictedFirstGoalMinute = firstGoalMinute,
            PredictedMvpPlayerId = mvpPlayerId,
            Status = isDraft ? PredictionEntryStatus.Draft : PredictionEntryStatus.Submitted
        };
    }

    public void UpdateDraft(
        int? homeScore,
        int? awayScore,
        int? halfTimeHomeScore = null,
        int? halfTimeAwayScore = null,
        bool? btts = null,
        decimal? overUnder = null,
        int? cards = null,
        int? corners = null,
        int? firstGoalMinute = null,
        Guid? mvpPlayerId = null,
        bool submit = false,
        DateTime? kickoffAt = null)
    {
        // Must be at least 1 minute before game starts
        if (kickoffAt.HasValue && DateTime.UtcNow >= kickoffAt.Value.AddMinutes(-1))
        {
            throw new InvalidOperationException("Prediction cannot be modified within 1 minute before kickoff.");
        }

        PredictedHomeScore = homeScore;
        PredictedAwayScore = awayScore;
        PredictedHalfTimeHomeScore = halfTimeHomeScore;
        PredictedHalfTimeAwayScore = halfTimeAwayScore;
        BothTeamsToScore = btts;
        TotalGoalsOverUnder = overUnder;
        PredictedTotalYellowCards = cards;
        PredictedTotalCorners = corners;
        PredictedFirstGoalMinute = firstGoalMinute;
        PredictedMvpPlayerId = mvpPlayerId;
        Status = submit ? PredictionEntryStatus.Submitted : PredictionEntryStatus.Draft;
    }

    public void Lock()
    {
        if (Status != PredictionEntryStatus.Scored)
        {
            Status = PredictionEntryStatus.Locked;
        }
    }

    public void ApplyScore(int points)
    {
        PointsEarned = points;
        IsScored = true;
        Status = PredictionEntryStatus.Scored;
        ScoredAt = DateTime.UtcNow;
    }
}
