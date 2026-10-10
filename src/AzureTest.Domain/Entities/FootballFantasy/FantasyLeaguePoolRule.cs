using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class FantasyLeaguePoolRule : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int ScopeValueMaxLength = 100;
    }

    public Guid FantasyLeagueId { get; private set; }
    public FantasyLeague FantasyLeague { get; private set; } = default!;
    public PoolScopeType ScopeType { get; private set; }
    public string ScopeValue { get; private set; } = default!;
    public bool IsExclude { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private FantasyLeaguePoolRule() { }

    public static FantasyLeaguePoolRule Create(Guid fantasyLeagueId, PoolScopeType scopeType, string scopeValue, bool isExclude = false)
    {
        if (fantasyLeagueId == Guid.Empty) throw new ArgumentException("FantasyLeagueId is required.", nameof(fantasyLeagueId));
        if (string.IsNullOrWhiteSpace(scopeValue) || scopeValue.Length > Rules.ScopeValueMaxLength)
            throw new ArgumentException("Invalid scope value.", nameof(scopeValue));

        return new FantasyLeaguePoolRule
        {
            FantasyLeagueId = fantasyLeagueId,
            ScopeType = scopeType,
            ScopeValue = scopeValue.Trim(),
            IsExclude = isExclude
        };
    }
}
