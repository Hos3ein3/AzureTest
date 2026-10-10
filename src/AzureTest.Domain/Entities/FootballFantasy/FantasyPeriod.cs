using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyPeriod : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
    }

    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public int Number { get; private set; }
    public string Name { get; private set; } = default!;
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }
    public DateTime LineupDeadline { get; private set; }
    public FantasyPeriodStatus Status { get; private set; }

    public ICollection<FantasyPeriodFixture> PeriodFixtures { get; private set; } = new List<FantasyPeriodFixture>();
    public ICollection<FantasyLineup> Lineups { get; private set; } = new List<FantasyLineup>();
    public ICollection<FantasyPlayerScore> PlayerScores { get; private set; } = new List<FantasyPlayerScore>();
    public ICollection<FantasyTeamPeriodScore> TeamPeriodScores { get; private set; } = new List<FantasyTeamPeriodScore>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyPeriod() { }

    public static FantasyPeriod Create(
        Guid fantasyLeagueId,
        int number,
        string name,
        DateTime startsAt,
        DateTime endsAt,
        DateTime lineupDeadline)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid period name.", nameof(name));
        if (lineupDeadline > startsAt)
            throw new ArgumentException("LineupDeadline must be on or before StartsAt.");

        return new FantasyPeriod
        {
            FantasyLeagueId = fantasyLeagueId,
            Number = number,
            Name = name.Trim(),
            StartsAt = startsAt,
            EndsAt = endsAt,
            LineupDeadline = lineupDeadline,
            Status = FantasyPeriodStatus.Upcoming
        };
    }

    public void Open() => Status = FantasyPeriodStatus.Open;
    public void Lock() => Status = FantasyPeriodStatus.Locked;
    public void Score() => Status = FantasyPeriodStatus.Scored;
}
