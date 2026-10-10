using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLeagueMember : BaseEntity, ISoftDelete, IAudit
{
    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public FantasyMemberRole Role { get; private set; }
    public FantasyMemberStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLeagueMember() { }

    public static FantasyLeagueMember Create(
        Guid fantasyLeagueId,
        Guid userId,
        FantasyMemberRole role = FantasyMemberRole.Member,
        FantasyMemberStatus status = FantasyMemberStatus.Active,
        DateTime? joinedAt = null)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (userId == Guid.Empty) throw new ArgumentException("UserId is required.", nameof(userId));

        return new FantasyLeagueMember
        {
            FantasyLeagueId = fantasyLeagueId,
            UserId = userId,
            Role = role,
            Status = status,
            JoinedAt = joinedAt ?? DateTime.UtcNow
        };
    }

    public void Approve() => Status = FantasyMemberStatus.Active;
    public void Reject() => Status = FantasyMemberStatus.Rejected;
    public void Remove() => Status = FantasyMemberStatus.Removed;
    public void Leave() => Status = FantasyMemberStatus.Left;
    public void MakeSpectator() => Status = FantasyMemberStatus.Spectator;
}
