using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerSquad : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int ShirtNumberMin = 1;
        public const int ShirtNumberMax = 99;
    }

    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid TeamSeasonId { get; private set; }
    public TeamSeason TeamSeason { get; private set; } = default!;
    public int? ShirtNumber { get; private set; }
    public DateTime FromDate { get; private set; }
    public DateTime? ToDate { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerSquad() { }

    public static PlayerSquad Create(Guid playerId, Guid teamSeasonId, DateTime fromDate, int? shirtNumber = null, DateTime? toDate = null)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (teamSeasonId == Guid.Empty) throw new ArgumentException("TeamSeasonId is required.", nameof(teamSeasonId));

        return new PlayerSquad
        {
            PlayerId = playerId,
            TeamSeasonId = teamSeasonId,
            ShirtNumber = shirtNumber,
            FromDate = fromDate,
            ToDate = toDate
        };
    }

    public void EndContract(DateTime toDate)
    {
        if (toDate < FromDate) throw new ArgumentException("ToDate cannot precede FromDate.", nameof(toDate));
        ToDate = toDate;
    }
}
