using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyTeamPeriodScore : BaseEntity, ISoftDelete, IAudit
{
    public Guid FantasyTeamId { get; private set; }
    public FantasyTeam FantasyTeam { get; private set; } = default!;
    public Guid FantasyPeriodId { get; private set; }
    public FantasyPeriod FantasyPeriod { get; private set; } = default!;
    public int Points { get; private set; }
    public int? Rank { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyTeamPeriodScore() { }

    public static FantasyTeamPeriodScore Create(
        Guid fantasyTeamId,
        Guid fantasyPeriodId,
        int points,
        int? rank = null)
    {
        if (fantasyTeamId == Guid.Empty) throw new ArgumentException("FantasyTeamId is required.", nameof(fantasyTeamId));
        if (fantasyPeriodId == Guid.Empty) throw new ArgumentException("FantasyPeriodId is required.", nameof(fantasyPeriodId));

        return new FantasyTeamPeriodScore
        {
            FantasyTeamId = fantasyTeamId,
            FantasyPeriodId = fantasyPeriodId,
            Points = points,
            Rank = rank
        };
    }

    public void UpdateScore(int points, int? rank = null)
    {
        Points = points;
        Rank = rank;
    }
}
