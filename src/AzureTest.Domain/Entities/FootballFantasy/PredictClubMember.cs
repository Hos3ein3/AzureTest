using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

/// <summary>
/// Predict Club Member - represents a user's membership and accumulated prediction points in the club.
/// </summary>
public sealed class PredictClubMember : BaseEntity, ISoftDelete, IAudit
{
    public Guid PredictClubId { get; private set; }
    public PredictClub PredictClub { get; private set; } = default!;

    public Guid UserId { get; private set; }
    public FantasyMemberRole Role { get; private set; }
    public FantasyMemberStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }

    // Leaderboard Points
    public int TotalPoints { get; private set; }
    public int CorrectExactScores { get; private set; }
    public int CorrectOutcomes { get; private set; }
    public int TotalPredictionsCount { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PredictClubMember() { }

    public static PredictClubMember Create(
        Guid predictClubId,
        Guid userId,
        FantasyMemberRole role = FantasyMemberRole.Member,
        FantasyMemberStatus status = FantasyMemberStatus.Active)
    {
        if (predictClubId == Guid.Empty) throw new ArgumentException("PredictClubId is required.", nameof(predictClubId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));

        return new PredictClubMember
        {
            PredictClubId = predictClubId,
            UserId = userId,
            Role = role,
            Status = status,
            JoinedAt = DateTime.UtcNow,
            TotalPoints = 0,
            CorrectExactScores = 0,
            CorrectOutcomes = 0,
            TotalPredictionsCount = 0
        };
    }

    public void AddPoints(int points, bool isExactScore, bool isOutcome)
    {
        TotalPoints += points;
        TotalPredictionsCount++;
        if (isExactScore) CorrectExactScores++;
        if (isOutcome) CorrectOutcomes++;
    }

    public void UpdateStatus(FantasyMemberStatus status)
    {
        Status = status;
    }
}
