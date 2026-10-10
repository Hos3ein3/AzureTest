using System;
using System.Collections.Generic;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class ScoringRuleSet : AggregateRoot, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
    }

    public string Name { get; private set; } = default!;
    public Guid? OwnerUserId { get; private set; }
    public bool IsTemplate { get; private set; }

    public ICollection<ScoringRule> RulesList { get; private set; } = new List<ScoringRule>();
    public ICollection<FantasyLeague> FantasyLeagues { get; private set; } = new List<FantasyLeague>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private ScoringRuleSet() { }

    public static ScoringRuleSet Create(string name, Guid? ownerUserId = null, bool isTemplate = false)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid scoring rule set name.", nameof(name));

        return new ScoringRuleSet
        {
            Name = name.Trim(),
            OwnerUserId = ownerUserId,
            IsTemplate = isTemplate
        };
    }
}
