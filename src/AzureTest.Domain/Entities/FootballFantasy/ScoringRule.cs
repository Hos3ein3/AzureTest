using System;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class ScoringRule : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int PointsMin = -50;
        public const int PointsMax = 50;
    }

    public Guid RuleSetId { get; private set; }
    public ScoringRuleSet RuleSet { get; private set; } = default!;
    public ScoringMetric Metric { get; private set; }
    public PlayerPosition? Position { get; private set; }
    public int Points { get; private set; }
    public int? Threshold { get; private set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private ScoringRule() { }

    public static ScoringRule Create(
        Guid ruleSetId,
        ScoringMetric metric,
        int points,
        PlayerPosition? position = null,
        int? threshold = null)
    {
        if (ruleSetId == Guid.Empty) throw new ArgumentException("RuleSetId is required.", nameof(ruleSetId));

        return new ScoringRule
        {
            RuleSetId = ruleSetId,
            Metric = metric,
            Points = points,
            Position = position,
            Threshold = threshold
        };
    }
}
