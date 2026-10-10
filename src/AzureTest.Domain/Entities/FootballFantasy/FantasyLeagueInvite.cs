using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLeagueInvite : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int EmailMaxLength = 200;
        public const int TokenMaxLength = 100;
    }

    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public Guid InvitedByUserId { get; private set; }
    public string? Email { get; private set; }
    public string Token { get; private set; } = default!;
    public DateTime ExpiresAt { get; private set; }
    public FantasyInviteStatus Status { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLeagueInvite() { }

    public static FantasyLeagueInvite Create(
        Guid fantasyLeagueId,
        Guid invitedByUserId,
        string token,
        DateTime expiresAt,
        string? email = null)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (invitedByUserId == Guid.Empty) throw new ArgumentException("InvitedByUserId is required.", nameof(invitedByUserId));
        if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("Token is required.", nameof(token));

        return new FantasyLeagueInvite
        {
            FantasyLeagueId = fantasyLeagueId,
            InvitedByUserId = invitedByUserId,
            Email = email?.Trim(),
            Token = token.Trim(),
            ExpiresAt = expiresAt,
            Status = FantasyInviteStatus.Pending
        };
    }

    public void Accept() => Status = FantasyInviteStatus.Accepted;
    public void Revoke() => Status = FantasyInviteStatus.Revoked;
    public void Expire() => Status = FantasyInviteStatus.Expired;
}
