using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Fixture : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int RoundMaxLength = 50;
        public const int VenueMaxLength = 150;
        public const int RefereeMaxLength = 100;
        public const int MinuteMin = 0;
        public const int MinuteMax = 150;
        public const int ScoreMin = 0;
    }

    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;
    public Guid? StageId { get; private set; }
    public Stage? Stage { get; private set; }
    public string? Round { get; private set; }
    public int? RoundNumber { get; private set; }
    public Guid HomeTeamId { get; private set; }
    public Team HomeTeam { get; private set; } = default!;
    public Guid AwayTeamId { get; private set; }
    public Team AwayTeam { get; private set; } = default!;
    public DateTime KickoffAt { get; private set; }
    public FixtureStatus Status { get; private set; }
    public int? Minute { get; private set; }
    public int? HomeScore { get; private set; }
    public int? AwayScore { get; private set; }
    public int? HomeScorePen { get; private set; }
    public int? AwayScorePen { get; private set; }
    public string? Venue { get; private set; }
    public string? Referee { get; private set; }
    public bool IsStatsFinalized { get; private set; }

    public ICollection<FixtureEvent> Events { get; private set; } = new List<FixtureEvent>();
    public ICollection<PlayerMatchStat> PlayerStats { get; private set; } = new List<PlayerMatchStat>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Fixture() { }

    public static Fixture Create(
        Guid seasonId,
        Guid homeTeamId,
        Guid awayTeamId,
        DateTime kickoffAt,
        Guid? stageId = null,
        string? round = null,
        int? roundNumber = null,
        string? venue = null,
        string? referee = null)
    {
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));
        if (homeTeamId == Guid.Empty) throw new ArgumentException("HomeTeamId is required.", nameof(homeTeamId));
        if (awayTeamId == Guid.Empty) throw new ArgumentException("AwayTeamId is required.", nameof(awayTeamId));
        if (homeTeamId == awayTeamId) throw new ArgumentException("Home and Away teams must be distinct.");

        return new Fixture
        {
            SeasonId = seasonId,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            KickoffAt = kickoffAt,
            StageId = stageId,
            Round = round?.Trim(),
            RoundNumber = roundNumber,
            Venue = venue?.Trim(),
            Referee = referee?.Trim(),
            Status = FixtureStatus.Scheduled,
            IsStatsFinalized = false
        };
    }

    public void UpdateScore(int homeScore, int awayScore, int? minute = null, int? homePen = null, int? awayPen = null)
    {
        HomeScore = Math.Max(Rules.ScoreMin, homeScore);
        AwayScore = Math.Max(Rules.ScoreMin, awayScore);
        Minute = minute;
        HomeScorePen = homePen;
        AwayScorePen = awayPen;
    }

    public void UpdateStatus(FixtureStatus status)
    {
        Status = status;
    }

    public void UpdateDetails(DateTime kickoffAt, string? round = null, int? roundNumber = null, string? venue = null, string? referee = null)
    {
        KickoffAt = kickoffAt;
        Round = round?.Trim();
        RoundNumber = roundNumber;
        Venue = venue?.Trim();
        Referee = referee?.Trim();
    }

    public void FinalizeStats()
    {
        if (Status != FixtureStatus.Finished)
            throw new InvalidOperationException("Cannot finalize stats on an unfinished fixture.");
        IsStatsFinalized = true;
    }
}
