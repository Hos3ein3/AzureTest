using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class PlayerTransfer : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int FeeMaxLength = 100;
    }

    public Guid PlayerId { get; private set; }
    public Player Player { get; private set; } = default!;
    public Guid? FromTeamId { get; private set; }
    public Team? FromTeam { get; private set; }
    public Guid? ToTeamId { get; private set; }
    public Team? ToTeam { get; private set; }

    public DateTime TransferDate { get; private set; }
    public TransferType Type { get; private set; }
    public string? Fee { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private PlayerTransfer() { }

    public static PlayerTransfer Create(
        Guid playerId,
        DateTime transferDate,
        TransferType type,
        Guid? fromTeamId = null,
        Guid? toTeamId = null,
        string? fee = null)
    {
        if (playerId == Guid.Empty) throw new ArgumentException("PlayerId is required.", nameof(playerId));

        return new PlayerTransfer
        {
            PlayerId = playerId,
            TransferDate = transferDate,
            Type = type,
            FromTeamId = fromTeamId,
            ToTeamId = toTeamId,
            Fee = fee?.Trim()
        };
    }

    public void Update(
        DateTime transferDate,
        TransferType type,
        Guid? fromTeamId = null,
        Guid? toTeamId = null,
        string? fee = null)
    {
        TransferDate = transferDate;
        Type = type;
        FromTeamId = fromTeamId;
        ToTeamId = toTeamId;
        Fee = fee?.Trim();
    }
}
