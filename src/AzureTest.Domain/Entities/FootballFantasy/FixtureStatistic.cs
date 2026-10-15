using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FixtureStatistic : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int TypeMaxLength = 50;
        public const int ValueMaxLength = 50;
    }

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;

    public string Type { get; private set; } = default!; // e.g. "Shots on Goal", "Fouls", "Corner Kicks", "Ball Possession"
    public string Value { get; private set; } = default!; // e.g. "5", "58%"

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FixtureStatistic() { }

    public static FixtureStatistic Create(Guid fixtureId, Guid teamId, string type, string value)
    {
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Type is required.", nameof(type));

        return new FixtureStatistic
        {
            FixtureId = fixtureId,
            TeamId = teamId,
            Type = type.Trim(),
            Value = value?.Trim() ?? "0"
        };
    }

    public void Update(string type, string value)
    {
        Type = type.Trim();
        Value = value?.Trim() ?? "0";
    }
}
