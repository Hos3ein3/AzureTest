using System;
using System.Collections.Generic;
using AzureTest.Domain.Entities.FootballFantasy.Enums;
using AzureTest.Domain.Primitives;

namespace AzureTest.Domain.Entities.FootballFantasy;

public sealed class Stage : BaseEntity, ISoftDelete, IAudit
{
    public static class Rules
    {
        public const int NameMinLength = 2;
        public const int NameMaxLength = 100;
        public const int MinOrder = 0;
    }

    public Guid SeasonId { get; private set; }
    public Season Season { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public StageType Type { get; private set; }
    public int Order { get; private set; }

    public ICollection<Fixture> Fixtures { get; private set; } = new List<Fixture>();

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // IAudit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }

    private Stage() { }

    public static Stage Create(Guid seasonId, string name, StageType type, int order)
    {
        if (seasonId == Guid.Empty) throw new ArgumentException("SeasonId is required.", nameof(seasonId));
        if (string.IsNullOrWhiteSpace(name) || name.Length > Rules.NameMaxLength)
            throw new ArgumentException("Invalid stage name.", nameof(name));

        return new Stage
        {
            SeasonId = seasonId,
            Name = name.Trim(),
            Type = type,
            Order = order
        };
    }
}
