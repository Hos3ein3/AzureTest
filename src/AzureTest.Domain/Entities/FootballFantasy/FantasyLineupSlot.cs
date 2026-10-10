using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLineupSlot : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int BenchOrderMin = 1;
        public const int BenchOrderMax = 10;
    }

    public Guid FantasyLineupId { get; private set; }
    public FantasyLineup FantasyLineup { get; private set; } = default!;
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public bool IsStarter { get; private set; }
    public int? BenchOrder { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLineupSlot() { }

    public static FantasyLineupSlot Create(
        Guid fantasyLineupId,
        Guid playerId,
        bool isStarter,
        int? benchOrder = null)
    {
        if (fantasyLineupId == Guid.Empty) throw new ArgumentException("FantasyLineupId is required.", nameof(fantasyLineupId));
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (isStarter && benchOrder.HasValue)
            throw new ArgumentException("Starters cannot have a bench order.");
        if (!isStarter && !benchOrder.HasValue)
            throw new ArgumentException("Bench players must have a bench order.");

        return new FantasyLineupSlot
        {
            FantasyLineupId = fantasyLineupId,
            PlayerId = playerId,
            IsStarter = isStarter,
            BenchOrder = benchOrder
        };
    }
}
