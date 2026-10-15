using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerSeasonStatistic : AggregateRoot, ISoftDelete, IAudit
{
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;

    public int Appearances { get; private set; }
    public int Lineups { get; private set; }
    public int MinutesPlayed { get; private set; }
    public int Goals { get; private set; }
    public int Assists { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }
    public decimal? Rating { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerSeasonStatistic() { }

    public static PlayerSeasonStatistic Create(
        Guid playerId,
        Guid seasonId,
        Guid teamId,
        int appearances,
        int lineups,
        int minutesPlayed,
        int goals,
        int assists,
        int yellowCards = 0,
        int redCards = 0,
        decimal? rating = null)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));

        return new PlayerSeasonStatistic
        {
            PlayerId = playerId,
            SeasonId = seasonId,
            TeamId = teamId,
            Appearances = appearances,
            Lineups = lineups,
            MinutesPlayed = minutesPlayed,
            Goals = goals,
            Assists = assists,
            YellowCards = yellowCards,
            RedCards = redCards,
            Rating = rating
        };
    }

    public void Update(
        int appearances,
        int lineups,
        int minutesPlayed,
        int goals,
        int assists,
        int yellowCards,
        int redCards,
        decimal? rating = null)
    {
        Appearances = appearances;
        Lineups = lineups;
        MinutesPlayed = minutesPlayed;
        Goals = goals;
        Assists = assists;
        YellowCards = yellowCards;
        RedCards = redCards;
        Rating = rating;
    }
}
