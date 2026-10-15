using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FixtureLiveUpdate : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int DescriptionMaxLength = 250;
        public const int StatusMaxLength = 50;
    }

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;

    public int ElapsedMinute { get; private set; }
    public int? ExtraMinute { get; private set; }
    public string Status { get; private set; } = default!; // "1H", "HT", "2H", "ET", "P", "FT"
    public int? HomeScore { get; private set; }
    public int? AwayScore { get; private set; }
    public string? Description { get; private set; }
    public DateTime Timestamp { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FixtureLiveUpdate() { }

    public static FixtureLiveUpdate Create(
        Guid fixtureId,
        int elapsedMinute,
        string status,
        int? homeScore = null,
        int? awayScore = null,
        int? extraMinute = null,
        string? description = null,
        DateTime? timestamp = null)
    {
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));

        return new FixtureLiveUpdate
        {
            FixtureId = fixtureId,
            ElapsedMinute = elapsedMinute,
            Status = status.Trim(),
            HomeScore = homeScore,
            AwayScore = awayScore,
            ExtraMinute = extraMinute,
            Description = description?.Trim(),
            Timestamp = timestamp ?? DateTime.UtcNow
        };
    }

    public void Update(int elapsedMinute, string status, int? homeScore, int? awayScore, string? description = null)
    {
        ElapsedMinute = elapsedMinute;
        Status = status.Trim();
        HomeScore = homeScore;
        AwayScore = awayScore;
        Description = description?.Trim();
        Timestamp = DateTime.UtcNow;
    }
}
