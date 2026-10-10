using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyTeamChip : BaseEntity, IAudit
{
    public Guid FantasyTeamId { get; private set; }
    public FantasyTeam FantasyTeam { get; private set; } = default!;
    public FantasyChipType ChipType { get; private set; }
    public Guid? UsedInPeriodId { get; private set; }
    public DateTime? UsedAt { get; private set; }
    public bool IsUsed { get; private set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyTeamChip() { }

    public static FantasyTeamChip Create(Guid fantasyTeamId, FantasyChipType chipType)
    {
        return new FantasyTeamChip
        {
            FantasyTeamId = fantasyTeamId,
            ChipType = chipType,
            IsUsed = false
        };
    }

    public void MarkUsed(Guid periodId)
    {
        if (IsUsed) throw new InvalidOperationException("Chip already used.");
        IsUsed = true;
        UsedInPeriodId = periodId;
        UsedAt = DateTime.UtcNow;
    }
}
