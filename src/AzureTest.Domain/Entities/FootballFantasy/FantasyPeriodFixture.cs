using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyPeriodFixture : BaseEntity, ISoftDelete, IAudit
{
    public Guid FantasyPeriodId { get; private set; }
    public FantasyPeriod FantasyPeriod { get; private set; } = default!;
    public Guid FixtureId { get; private set; }
    public Fixture Fixture { get; private set; } = default!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyPeriodFixture() { }

    public static FantasyPeriodFixture Create(Guid fantasyPeriodId, Guid fixtureId)
    {
        if (fantasyPeriodId == Guid.Empty) throw new ArgumentException("FantasyPeriodId is required.", nameof(fantasyPeriodId));
        if (fixtureId == Guid.Empty) throw new ArgumentException("FixtureId is required.", nameof(fixtureId));

        return new FantasyPeriodFixture
        {
            FantasyPeriodId = fantasyPeriodId,
            FixtureId = fixtureId
        };
    }
}
