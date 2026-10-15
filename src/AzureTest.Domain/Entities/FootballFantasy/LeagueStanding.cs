using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class LeagueStanding : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int DescriptionMaxLength = 100;
        public const int FormMaxLength = 10;
        public const int StatusMaxLength = 20;
    }

    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;

    public int Rank { get; private set; }
    public int Points { get; private set; }
    public int GoalsDiff { get; private set; }
    public string? Form { get; private set; }
    public string? Status { get; private set; }
    public string? Description { get; private set; }

    public int Played { get; private set; }
    public int Won { get; private set; }
    public int Drawn { get; private set; }
    public int Lost { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private LeagueStanding() { }

    public static LeagueStanding Create(
        Guid seasonId,
        Guid teamId,
        int rank,
        int points,
        int played,
        int won,
        int drawn,
        int lost,
        int goalsFor,
        int goalsAgainst,
        string? form = null,
        string? description = null,
        string? status = null)
    {
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));

        return new LeagueStanding
        {
            SeasonId = seasonId,
            TeamId = teamId,
            Rank = rank,
            Points = points,
            Played = played,
            Won = won,
            Drawn = drawn,
            Lost = lost,
            GoalsFor = goalsFor,
            GoalsAgainst = goalsAgainst,
            GoalsDiff = goalsFor - goalsAgainst,
            Form = form?.Trim(),
            Description = description?.Trim(),
            Status = status?.Trim()
        };
    }

    public void Update(
        int rank,
        int points,
        int played,
        int won,
        int drawn,
        int lost,
        int goalsFor,
        int goalsAgainst,
        string? form = null,
        string? description = null,
        string? status = null)
    {
        Rank = rank;
        Points = points;
        Played = played;
        Won = won;
        Drawn = drawn;
        Lost = lost;
        GoalsFor = goalsFor;
        GoalsAgainst = goalsAgainst;
        GoalsDiff = goalsFor - goalsAgainst;
        Form = form?.Trim();
        Description = description?.Trim();
        Status = status?.Trim();
    }
}
