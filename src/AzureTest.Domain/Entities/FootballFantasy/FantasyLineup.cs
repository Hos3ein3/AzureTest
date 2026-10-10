using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLineup : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int FormationMaxLength = 20;
    }

    public Guid FantasyTeamId { get; private set; }
    public FantasyTeam FantasyTeam { get; private set; } = default!;
    public Guid FantasyPeriodId { get; private set; }
    public FantasyPeriod FantasyPeriod { get; private set; } = default!;
    public string Formation { get; private set; } = default!;
    public Guid? CaptainPlayerId { get; private set; }
    public Player? CaptainPlayer { get; private set; }
    public Guid? ViceCaptainPlayerId { get; private set; }
    public Player? ViceCaptainPlayer { get; private set; }
    public bool IsLocked { get; private set; }

    public ICollection<FantasyLineupSlot> Slots { get; private set; } = new List<FantasyLineupSlot>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLineup() { }

    public static FantasyLineup Create(
        Guid fantasyTeamId,
        Guid fantasyPeriodId,
        string formation,
        Guid? captainPlayerId = null,
        Guid? viceCaptainPlayerId = null)
    {
        if (fantasyTeamId == Guid.Empty) throw new ArgumentException("FantasyTeamId is required.", nameof(fantasyTeamId));
        if (fantasyPeriodId == Guid.Empty) throw new ArgumentException("FantasyPeriodId is required.", nameof(fantasyPeriodId));
        if (string.IsNullOrWhiteSpace(formation)) throw new ArgumentException("Formation is required.", nameof(formation));
        if (captainPlayerId.HasValue && viceCaptainPlayerId.HasValue && captainPlayerId == viceCaptainPlayerId)
            throw new ArgumentException("Captain and ViceCaptain cannot be the same player.");

        return new FantasyLineup
        {
            FantasyTeamId = fantasyTeamId,
            FantasyPeriodId = fantasyPeriodId,
            Formation = formation.Trim(),
            CaptainPlayerId = captainPlayerId,
            ViceCaptainPlayerId = viceCaptainPlayerId,
            IsLocked = false
        };
    }

    public void Lock() => IsLocked = true;
    public void Unlock() => IsLocked = false;

    public void AssignCaptains(Guid captainPlayerId, Guid viceCaptainPlayerId)
    {
        if (IsLocked) throw new InvalidOperationException("Cannot alter a locked lineup.");
        if (captainPlayerId == viceCaptainPlayerId) throw new ArgumentException("Captain and ViceCaptain cannot be identical.");
        CaptainPlayerId = captainPlayerId;
        ViceCaptainPlayerId = viceCaptainPlayerId;
    }

    public void UpdateFormation(string formation, Guid? captainPlayerId = null, Guid? viceCaptainPlayerId = null)
    {
        if (IsLocked) throw new InvalidOperationException("Cannot alter a locked lineup.");
        if (!string.IsNullOrWhiteSpace(formation)) Formation = formation.Trim();
        CaptainPlayerId = captainPlayerId;
        ViceCaptainPlayerId = viceCaptainPlayerId;
    }
}
