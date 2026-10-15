using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FixtureLineup : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int FormationMaxLength = 20;
    }

    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;
    public Guid? CoachId { get; private set; }
    public Coach? Coach { get; private set; }

    public string Formation { get; private set; } = default!; // e.g. "4-3-3", "4-2-3-1"

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FixtureLineup() { }

    public static FixtureLineup Create(Guid fixtureId, Guid teamId, string formation, Guid? coachId = null)
    {
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));
        if (string.IsNullOrWhiteSpace(formation)) throw new ArgumentException("Formation is required.", nameof(formation));

        return new FixtureLineup
        {
            FixtureId = fixtureId,
            TeamId = teamId,
            Formation = formation.Trim(),
            CoachId = coachId
        };
    }

    public void Update(string formation, Guid? coachId = null)
    {
        Formation = formation.Trim();
        CoachId = coachId;
    }
}
