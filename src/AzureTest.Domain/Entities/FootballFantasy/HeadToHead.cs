using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class HeadToHead : AggregateRoot, ISoftDelete, IAudit
{
    public Guid Team1Id { get; private set; }
    public Team Team1 { get; private set; } = default!;
    public Guid Team2Id { get; private set; }
    public Team Team2 { get; private set; } = default!;

    public int TotalPlayed { get; private set; }
    public int Team1Wins { get; private set; }
    public int Team2Wins { get; private set; }
    public int Draws { get; private set; }
    public int Team1Goals { get; private set; }
    public int Team2Goals { get; private set; }
    public DateTime LastUpdated { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private HeadToHead() { }

    public static HeadToHead Create(
        Guid team1Id,
        Guid team2Id,
        int totalPlayed,
        int team1Wins,
        int team2Wins,
        int draws,
        int team1Goals,
        int team2Goals)
    {
        if (team1Id == Guid.Empty) throw new ArgumentException("Team1Id is required.", nameof(team1Id));
        if (team2Id == Guid.Empty) throw new ArgumentException("Team2Id is required.", nameof(team2Id));
        if (team1Id == team2Id) throw new ArgumentException("Teams must be distinct.");

        return new HeadToHead
        {
            Team1Id = team1Id,
            Team2Id = team2Id,
            TotalPlayed = totalPlayed,
            Team1Wins = team1Wins,
            Team2Wins = team2Wins,
            Draws = draws,
            Team1Goals = team1Goals,
            Team2Goals = team2Goals,
            LastUpdated = DateTime.UtcNow
        };
    }

    public void Update(int totalPlayed, int team1Wins, int team2Wins, int draws, int team1Goals, int team2Goals)
    {
        TotalPlayed = totalPlayed;
        Team1Wins = team1Wins;
        Team2Wins = team2Wins;
        Draws = draws;
        Team1Goals = team1Goals;
        Team2Goals = team2Goals;
        LastUpdated = DateTime.UtcNow;
    }
}
