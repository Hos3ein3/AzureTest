using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyMatchup : BaseEntity, IAudit
{
    public static class Rules
    {
        public const int StageNameMaxLength = 50;
    }

    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;

    public Guid? FantasyLeagueGroupId { get; private set; }
    public FantasyLeagueGroup? FantasyLeagueGroup { get; private set; }

    public Guid? PeriodId { get; private set; }
    public FantasyPeriod? Period { get; private set; }

    public int RoundNumber { get; private set; }
    public string StageName { get; private set; } = default!; // e.g. "Group Stage", "Quarter-Final", "Semi-Final", "Final"
    public int MatchNumber { get; private set; }

    public Guid? HomeTeamId { get; private set; }
    public FantasyTeam? HomeTeam { get; private set; }
    public decimal HomeScore { get; private set; }

    public Guid? AwayTeamId { get; private set; }
    public FantasyTeam? AwayTeam { get; private set; }
    public decimal AwayScore { get; private set; }

    public Guid? WinnerTeamId { get; private set; }
    public FantasyTeam? WinnerTeam { get; private set; }

    public bool IsCompleted { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyMatchup() { }

    public static FantasyMatchup Create(
        Guid fantasyLeagueId,
        int roundNumber,
        string stageName,
        int matchNumber,
        Guid? homeTeamId = null,
        Guid? awayTeamId = null,
        Guid? fantasyLeagueGroupId = null,
        Guid? periodId = null)
    {
        return new FantasyMatchup
        {
            FantasyLeagueId = fantasyLeagueId,
            RoundNumber = roundNumber,
            StageName = stageName.Trim(),
            MatchNumber = matchNumber,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            FantasyLeagueGroupId = fantasyLeagueGroupId,
            PeriodId = periodId,
            HomeScore = 0,
            AwayScore = 0,
            IsCompleted = false
        };
    }

    public void UpdateScore(decimal homeScore, decimal awayScore, bool isCompleted = true)
    {
        HomeScore = homeScore;
        AwayScore = awayScore;
        IsCompleted = isCompleted;

        if (isCompleted)
        {
            if (HomeScore > AwayScore) WinnerTeamId = HomeTeamId;
            else if (AwayScore > HomeScore) WinnerTeamId = AwayTeamId;
            else WinnerTeamId = null; // Draw
        }
    }

    public void SetTeams(Guid? homeTeamId, Guid? awayTeamId)
    {
        HomeTeamId = homeTeamId;
        AwayTeamId = awayTeamId;
    }
}
