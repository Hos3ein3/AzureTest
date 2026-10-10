using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class TeamCoach : BaseEntity, ISoftDelete, IAudit
{
    public Guid CoachId { get; private set; }
    public Coach Coach { get; private set; } = default!;
    public Guid TeamId { get; private set; }
    public Team Team { get; private set; } = default!;
    public CoachRole Role { get; private set; }
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

    private TeamCoach() { }

    public static TeamCoach Create(Guid coachId, Guid teamId, CoachRole role, DateTime fromDate, DateTime? toDate = null)
    {
        if (coachId == Guid.Empty) throw new ArgumentException("CoachId is required.", nameof(coachId));
        if (teamId == Guid.Empty) throw new ArgumentException("TeamId is required.", nameof(teamId));

        return new TeamCoach
        {
            CoachId = coachId,
            TeamId = teamId,
            Role = role,
            FromDate = fromDate,
            ToDate = toDate
        };
    }

    public void EndTenure(DateTime toDate)
    {
        if (toDate < FromDate) throw new ArgumentException("ToDate cannot precede FromDate.", nameof(toDate));
        ToDate = toDate;
    }
}
