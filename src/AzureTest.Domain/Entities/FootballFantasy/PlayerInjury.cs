using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerInjury : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int TypeMaxLength = 100;
        public const int ReasonMaxLength = 200;
    }

    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid? FixtureId { get; private set; }
    public Fixture? Fixture { get; private set; }

    public string Type { get; private set; } = default!;
    public string? Reason { get; private set; }
    public InjurySeverity Severity { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? ExpectedReturnDate { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerInjury() { }

    public static PlayerInjury Create(
        Guid playerId,
        string type,
        InjurySeverity severity,
        DateTime startDate,
        DateTime? expectedReturnDate = null,
        string? reason = null,
        Guid? fixtureId = null)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Injury type is required.", nameof(type));

        return new PlayerInjury
        {
            PlayerId = playerId,
            Type = type.Trim(),
            Severity = severity,
            StartDate = startDate,
            ExpectedReturnDate = expectedReturnDate,
            Reason = reason?.Trim(),
            FixtureId = fixtureId
        };
    }

    public void Update(
        string type,
        InjurySeverity severity,
        DateTime startDate,
        DateTime? expectedReturnDate = null,
        string? reason = null)
    {
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Injury type is required.", nameof(type));

        Type = type.Trim();
        Severity = severity;
        StartDate = startDate;
        ExpectedReturnDate = expectedReturnDate;
        Reason = reason?.Trim();
    }
}
