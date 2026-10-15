using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerTopScorer : AggregateRoot, ISoftDelete, IAudit
{
    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;

    public int Rank { get; private set; }
    public int Goals { get; private set; }
    public int? Assists { get; private set; }
    public int? PenaltyGoals { get; private set; }
    public int? MatchesPlayed { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerTopScorer() { }

    public static PlayerTopScorer Create(
        Guid seasonId,
        Guid playerId,
        Guid teamId,
        int rank,
        int goals,
        int? assists = null,
        int? penaltyGoals = null,
        int? matchesPlayed = null)
    {
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));

        return new PlayerTopScorer
        {
            SeasonId = seasonId,
            PlayerId = playerId,
            TeamId = teamId,
            Rank = rank,
            Goals = Math.Max(0, goals),
            Assists = assists,
            PenaltyGoals = penaltyGoals,
            MatchesPlayed = matchesPlayed
        };
    }

    public void Update(int rank, int goals, int? assists, int? penaltyGoals, int? matchesPlayed)
    {
        Rank = rank;
        Goals = Math.Max(0, goals);
        Assists = assists;
        PenaltyGoals = penaltyGoals;
        MatchesPlayed = matchesPlayed;
    }
}
