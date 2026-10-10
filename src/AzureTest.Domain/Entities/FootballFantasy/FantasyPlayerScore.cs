using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyPlayerScore : BaseEntity, ISoftDelete, IAudit
{
    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public Guid FantasyPeriodId { get; private set; }
    public FantasyPeriod FantasyPeriod { get; private set; } = default!;
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;
    public int Points { get; private set; }
    public string Breakdown { get; private set; } = default!; // JSONB breakdown of scoring

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyPlayerScore() { }

    public static FantasyPlayerScore Create(
        Guid fantasyLeagueId,
        Guid fantasyPeriodId,
        Guid playerId,
        Guid fixtureId,
        int points,
        string breakdown)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (fantasyPeriodId == Guid.Empty) throw new ArgumentException("FantasyPeriodId is required.", nameof(fantasyPeriodId));
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));

        return new FantasyPlayerScore
        {
            FantasyLeagueId = fantasyLeagueId,
            FantasyPeriodId = fantasyPeriodId,
            PlayerId = playerId,
            FixtureId = fixtureId,
            Points = points,
            Breakdown = string.IsNullOrWhiteSpace(breakdown) ? "{}" : breakdown.Trim()
        };
    }

    public void UpdateScore(int points, string breakdown)
    {
        Points = points;
        Breakdown = string.IsNullOrWhiteSpace(breakdown) ? "{}" : breakdown.Trim();
    }
}
