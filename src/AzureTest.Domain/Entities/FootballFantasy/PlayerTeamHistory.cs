using System;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerTeamHistory : AggregateRoot, ISoftDelete, IAudit
{
    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;

    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int? JerseyNumber { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerTeamHistory() { }

    public static PlayerTeamHistory Create(
        Guid playerId,
        Guid teamId,
        DateTime startDate,
        DateTime? endDate = null,
        int? jerseyNumber = null)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));

        return new PlayerTeamHistory
        {
            PlayerId = playerId,
            TeamId = teamId,
            StartDate = startDate,
            EndDate = endDate,
            JerseyNumber = jerseyNumber
        };
    }

    public void Update(DateTime startDate, DateTime? endDate = null, int? jerseyNumber = null)
    {
        StartDate = startDate;
        EndDate = endDate;
        JerseyNumber = jerseyNumber;
    }
}
