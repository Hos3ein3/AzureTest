using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FixtureEvent : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int MinuteMin = 0;
        public const int MinuteMax = 150;
        public const int DetailMaxLength = 200;
    }

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;
    public Guid? PlayerId { get; private set; }
    public Player? Player { get; private set; }
    public Guid? RelatedPlayerId { get; private set; }
    public Player? RelatedPlayer { get; private set; }
    public FixtureEventType Type { get; private set; }
    public int Minute { get; private set; }
    public int? ExtraMinute { get; private set; }
    public string? Detail { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FixtureEvent() { }

    public static FixtureEvent Create(
        Guid fixtureId,
        Guid teamId,
        FixtureEventType type,
        int minute,
        Guid? playerId = null,
        Guid? relatedPlayerId = null,
        int? extraMinute = null,
        string? detail = null)
    {
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));

        return new FixtureEvent
        {
            FixtureId = fixtureId,
            TeamId = teamId,
            Type = type,
            Minute = minute,
            PlayerId = playerId,
            RelatedPlayerId = relatedPlayerId,
            ExtraMinute = extraMinute,
            Detail = detail?.Trim()
        };
    }
}
