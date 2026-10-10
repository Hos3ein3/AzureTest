using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLeagueSource : BaseEntity, ISoftDelete, IAudit
{
    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLeagueSource() { }

    public static FantasyLeagueSource Create(Guid fantasyLeagueId, Guid seasonId)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));

        return new FantasyLeagueSource
        {
            FantasyLeagueId = fantasyLeagueId,
            SeasonId = seasonId
        };
    }
}
