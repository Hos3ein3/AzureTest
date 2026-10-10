using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasySquadEntry : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int PricePrecision = 10;
        public const int PriceScale = 2;
    }

    public Guid FantasyTeamId { get; private set; }
    public FantasyTeam FantasyTeam { get; private set; } = default!;
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public decimal PricePaid { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasySquadEntry() { }

    public static FantasySquadEntry Create(Guid fantasyTeamId, Guid playerId, decimal pricePaid)
    {
        if (fantasyTeamId == Guid.Empty) throw new ArgumentException("FantasyTeamId is required.", nameof(fantasyTeamId));
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (pricePaid < 0) throw new ArgumentException("PricePaid cannot be negative.", nameof(pricePaid));

        return new FantasySquadEntry
        {
            FantasyTeamId = fantasyTeamId,
            PlayerId = playerId,
            PricePaid = pricePaid
        };
    }
}
