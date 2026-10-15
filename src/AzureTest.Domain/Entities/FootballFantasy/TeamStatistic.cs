using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class TeamStatistic : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int FormMaxLength = 20;
    }

    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;
    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;

    public int MatchesPlayed { get; private set; }
    public int Wins { get; private set; }
    public int Draws { get; private set; }
    public int Losses { get; private set; }
    public int GoalsFor { get; private set; }
    public int GoalsAgainst { get; private set; }
    public int CleanSheets { get; private set; }
    public int FailedToScore { get; private set; }
    public int YellowCards { get; private set; }
    public int RedCards { get; private set; }
    public string? Form { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private TeamStatistic() { }

    public static TeamStatistic Create(
        Guid teamId,
        Guid seasonId,
        int matchesPlayed,
        int wins,
        int draws,
        int losses,
        int goalsFor,
        int goalsAgainst,
        int cleanSheets = 0,
        int failedToScore = 0,
        int yellowCards = 0,
        int redCards = 0,
        string? form = null)
    {
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));

        return new TeamStatistic
        {
            TeamId = teamId,
            SeasonId = seasonId,
            MatchesPlayed = matchesPlayed,
            Wins = wins,
            Draws = draws,
            Losses = losses,
            GoalsFor = goalsFor,
            GoalsAgainst = goalsAgainst,
            CleanSheets = cleanSheets,
            FailedToScore = failedToScore,
            YellowCards = yellowCards,
            RedCards = redCards,
            Form = form?.Trim()
        };
    }

    public void Update(
        int matchesPlayed,
        int wins,
        int draws,
        int losses,
        int goalsFor,
        int goalsAgainst,
        int cleanSheets,
        int failedToScore,
        int yellowCards,
        int redCards,
        string? form = null)
    {
        MatchesPlayed = matchesPlayed;
        Wins = wins;
        Draws = draws;
        Losses = losses;
        GoalsFor = goalsFor;
        GoalsAgainst = goalsAgainst;
        CleanSheets = cleanSheets;
        FailedToScore = failedToScore;
        YellowCards = yellowCards;
        RedCards = redCards;
        Form = form?.Trim();
    }
}
