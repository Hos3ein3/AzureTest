using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLeaguePositionLimit : BaseEntity, ISoftDelete, IAudit
{
    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public PlayerPosition Position { get; private set; }
    public int MinInSquad { get; private set; }
    public int MaxInSquad { get; private set; }
    public int MinInStarting { get; private set; }
    public int MaxInStarting { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLeaguePositionLimit() { }

    public static FantasyLeaguePositionLimit Create(
        Guid fantasyLeagueId,
        PlayerPosition position,
        int minInSquad,
        int maxInSquad,
        int minInStarting,
        int maxInStarting)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (minInSquad > maxInSquad) throw new ArgumentException("MinInSquad cannot exceed MaxInSquad.");
        if (minInStarting > maxInStarting) throw new ArgumentException("MinInStarting cannot exceed MaxInStarting.");
        if (minInStarting > minInSquad || maxInStarting > maxInSquad) throw new ArgumentException("Starting limits must fit within Squad limits.");

        return new FantasyLeaguePositionLimit
        {
            FantasyLeagueId = fantasyLeagueId,
            Position = position,
            MinInSquad = minInSquad,
            MaxInSquad = maxInSquad,
            MinInStarting = minInStarting,
            MaxInStarting = maxInStarting
        };
    }
}
