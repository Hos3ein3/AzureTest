using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

/// <summary>
/// User's tournament level predictions for a Predict Club (1st, 2nd, 3rd place, Top Scorer, MVP, Top Assister).
/// </summary>
public sealed class TournamentPrediction : BaseEntity, ISoftDelete, IAudit
{
    public Guid PredictClubId { get; private set; }
    public PredictClub PredictClub { get; private set; } = default!;

    public Guid UserId { get; private set; }
    public PredictionEntryStatus Status { get; private set; } = PredictionEntryStatus.Draft;

    // Podium Predictions
    public Guid? FirstPlaceTeamId { get; private set; }
    public Team? FirstPlaceTeam { get; private set; }

    public Guid? SecondPlaceTeamId { get; private set; }
    public Team? SecondPlaceTeam { get; private set; }

    public Guid? ThirdPlaceTeamId { get; private set; }
    public Team? ThirdPlaceTeam { get; private set; }

    // Individual Awards
    public Guid? BestPlayerId { get; private set; }
    public Player? BestPlayer { get; private set; }

    public Guid? TopScorerPlayerId { get; private set; }
    public Player? TopScorerPlayer { get; private set; }

    public Guid? TopAssisterPlayerId { get; private set; }
    public Player? TopAssisterPlayer { get; private set; }

    public int PointsEarned { get; private set; }
    public bool IsScored { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private TournamentPrediction() { }

    public static TournamentPrediction Create(
        Guid predictClubId,
        Guid userId,
        Guid? firstPlaceTeamId = null,
        Guid? secondPlaceTeamId = null,
        Guid? thirdPlaceTeamId = null,
        Guid? bestPlayerId = null,
        Guid? topScorerPlayerId = null,
        Guid? topAssisterPlayerId = null,
        bool isDraft = true)
    {
        if (predictClubId == Guid.Empty) throw new ArgumentException("PredictClubId is required.", nameof(predictClubId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));

        return new TournamentPrediction
        {
            PredictClubId = predictClubId,
            UserId = userId,
            FirstPlaceTeamId = firstPlaceTeamId,
            SecondPlaceTeamId = secondPlaceTeamId,
            ThirdPlaceTeamId = thirdPlaceTeamId,
            BestPlayerId = bestPlayerId,
            TopScorerPlayerId = topScorerPlayerId,
            TopAssisterPlayerId = topAssisterPlayerId,
            Status = isDraft ? PredictionEntryStatus.Draft : PredictionEntryStatus.Submitted
        };
    }

    public void UpdateDraft(
        Guid? firstPlaceTeamId,
        Guid? secondPlaceTeamId,
        Guid? thirdPlaceTeamId,
        Guid? bestPlayerId,
        Guid? topScorerPlayerId,
        Guid? topAssisterPlayerId,
        bool submit = false)
    {
        FirstPlaceTeamId = firstPlaceTeamId;
        SecondPlaceTeamId = secondPlaceTeamId;
        ThirdPlaceTeamId = thirdPlaceTeamId;
        BestPlayerId = bestPlayerId;
        TopScorerPlayerId = topScorerPlayerId;
        TopAssisterPlayerId = topAssisterPlayerId;
        Status = submit ? PredictionEntryStatus.Submitted : PredictionEntryStatus.Draft;
    }

    public void ApplyScore(int points)
    {
        PointsEarned = points;
        IsScored = true;
        Status = PredictionEntryStatus.Scored;
    }
}
