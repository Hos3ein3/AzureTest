using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class TeamTrophy : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int LeagueNameMaxLength = 150;
        public const int SeasonMaxLength = 50;
        public const int PlaceMaxLength = 50;
    }

    public Guid? TeamId { get; private set; }
    public Team? Team { get; private set; }
    public Guid? PlayerId { get; private set; }
    public Player? Player { get; private set; }
    public Guid? CoachId { get; private set; }
    public Coach? Coach { get; private set; }

    public string LeagueName { get; private set; } = default!;
    public string Season { get; private set; } = default!;
    public string Place { get; private set; } = default!; // e.g. "Winner", "Runner-up"

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private TeamTrophy() { }

    public static TeamTrophy Create(
        string leagueName,
        string season,
        string place,
        Guid? teamId = null,
        Guid? playerId = null,
        Guid? coachId = null)
    {
        if (string.IsNullOrWhiteSpace(leagueName)) throw new ArgumentException("LeagueName is required.", nameof(leagueName));
        if (string.IsNullOrWhiteSpace(season)) throw new ArgumentException("Season is required.", nameof(season));
        if (string.IsNullOrWhiteSpace(place)) throw new ArgumentException("Place is required.", nameof(place));

        return new TeamTrophy
        {
            LeagueName = leagueName.Trim(),
            Season = season.Trim(),
            Place = place.Trim(),
            TeamId = teamId,
            PlayerId = playerId,
            CoachId = coachId
        };
    }

    public void Update(string leagueName, string season, string place)
    {
        LeagueName = leagueName.Trim();
        Season = season.Trim();
        Place = place.Trim();
    }
}
